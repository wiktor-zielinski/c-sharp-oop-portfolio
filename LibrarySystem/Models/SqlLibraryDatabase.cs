using LibrarySystem.Core;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibrarySystem.Models
{
    public class SqlLibraryDatabase : ILibraryDatabase
    {
        private readonly string _connectionString;

        public SqlLibraryDatabase(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IReadOnlyList<Book> ListOfBooks => FetchAllBooks().AsReadOnly();

        // Dodaje nową książkę do bazy SQL, sprawdzając unikalność numeru ISBN
        public void Add(Book book)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            if (GetByIsbn(book.Isbn) != null) throw new InvalidOperationException($"Książka o ISBN {book.Isbn} już istnieje w bazie.");

            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                const string insertBookSql = @"
                    INSERT INTO dbo.Books (Isbn, Title, Author, PublicationDate, Status, DueDate, BorrowedByUserId, ReservedByUserId)
                    VALUES (@Isbn, @Title, @Author, @PublicationDate, @Status, @DueDate, @BorrowedByUserId, @ReservedByUserId);";

                using (var cmd = new SqlCommand(insertBookSql, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@Isbn", book.Isbn);
                    cmd.Parameters.AddWithValue("@Title", book.Title);
                    cmd.Parameters.AddWithValue("@Author", book.Author);
                    cmd.Parameters.AddWithValue("@PublicationDate", book.PublicationDate.Date);
                    cmd.Parameters.AddWithValue("@Status", (int)book.Status);
                    cmd.Parameters.AddWithValue("@DueDate", (object?)book.DueDate?.Date ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@BorrowedByUserId", (object?)book.BorrowedByUserId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ReservedByUserId", (object?)book.ReservedByUserId ?? DBNull.Value);

                    cmd.ExecuteNonQuery();
                }

                InsertNewBookFeedbacks(book, 0, conn, transaction);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // Usuwa książkę z bazy (opinie usuwają się automatycznie przez ON DELETE CASCADE)
        public void Delete(Book book)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            if (book.Status == BookStatus.Borrowed) throw new InvalidOperationException("Nie można usunąć książki, która jest aktualnie wypożyczona.");

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            const string sql = "DELETE FROM dbo.Books WHERE Isbn = @Isbn;";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Isbn", book.Isbn);
            cmd.ExecuteNonQuery();
        }

        // Aktualizuje dane książki oraz dopisuje nowe opinie bez nadpisywania dat starych opinii
        public void Update(Book updatedBook)
        {
            if (updatedBook == null) throw new ArgumentNullException(nameof(updatedBook));

            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                const string updateSql = @"
                    UPDATE dbo.Books
                    SET Title = @Title,
                        Author = @Author,
                        PublicationDate = @PublicationDate,
                        Status = @Status,
                        DueDate = @DueDate,
                        BorrowedByUserId = @BorrowedByUserId,
                        ReservedByUserId = @ReservedByUserId
                    WHERE Isbn = @Isbn;";

                using (var cmd = new SqlCommand(updateSql, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@Isbn", updatedBook.Isbn);
                    cmd.Parameters.AddWithValue("@Title", updatedBook.Title);
                    cmd.Parameters.AddWithValue("@Author", updatedBook.Author);
                    cmd.Parameters.AddWithValue("@PublicationDate", updatedBook.PublicationDate.Date);
                    cmd.Parameters.AddWithValue("@Status", (int)updatedBook.Status);
                    cmd.Parameters.AddWithValue("@DueDate", (object?)updatedBook.DueDate?.Date ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@BorrowedByUserId", (object?)updatedBook.BorrowedByUserId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ReservedByUserId", (object?)updatedBook.ReservedByUserId ?? DBNull.Value);

                    int rows = cmd.ExecuteNonQuery();
                    if (rows == 0) throw new InvalidOperationException($"Nie można zaktualizować: Książka o ISBN {updatedBook.Isbn} nie istnieje w bazie.");
                }

                const string countFeedbacksSql = "SELECT COUNT(*) FROM dbo.BookFeedbacks WHERE BookIsbn = @BookIsbn;";
                int existingCount;
                using (var countCmd = new SqlCommand(countFeedbacksSql, conn, transaction))
                {
                    countCmd.Parameters.AddWithValue("@BookIsbn", updatedBook.Isbn);
                    existingCount = Convert.ToInt32(countCmd.ExecuteScalar());
                }

                InsertNewBookFeedbacks(updatedBook, existingCount, conn, transaction);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // Wyszukuje pojedynczą książkę po numerze ISBN
        public Book? GetByIsbn(string isbn)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            const string sql = @"
                SELECT Isbn, Title, Author, PublicationDate, Status, DueDate, BorrowedByUserId, ReservedByUserId
                FROM dbo.Books
                WHERE Isbn = @Isbn;";

            Book? book = null;
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Isbn", isbn);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    book = MapReaderToBook(reader);
                }
            }

            if (book != null)
            {
                LoadFeedbacksForBook(book, conn);
            }

            return book;
        }

        // Wyszukuje książki po tytule, autorze lub ISBN bezpośrednio po stronie SQL Servera
        public IEnumerable<Book> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return ListOfBooks;

            return ListOfBooks.Where(b =>
                b.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                b.Author.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                b.Isbn.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        private List<Book> FetchAllBooks()
        {
            var books = new List<Book>();
            var bookDict = new Dictionary<string, Book>(StringComparer.OrdinalIgnoreCase);

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            const string booksSql = @"
                SELECT Isbn, Title, Author, PublicationDate, Status, DueDate, BorrowedByUserId, ReservedByUserId
                FROM dbo.Books
                ORDER BY Title ASC;";

            using (var cmd = new SqlCommand(booksSql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var book = MapReaderToBook(reader);
                    books.Add(book);
                    bookDict[book.Isbn] = book;
                }
            }

            if (books.Count > 0)
            {
                const string feedbacksSql = @"
                    SELECT BookIsbn, CreatedAt, FeedbackText
                    FROM dbo.BookFeedbacks
                    ORDER BY FeedbackId ASC;";

                using var fbCmd = new SqlCommand(feedbacksSql, conn);
                using var fbReader = fbCmd.ExecuteReader();
                while (fbReader.Read())
                {
                    string isbn = fbReader.GetString(0);
                    string createdAt = fbReader.GetString(1);
                    string text = fbReader.GetString(2);

                    if (bookDict.TryGetValue(isbn, out var book))
                    {
                        book.LoadFeedbackFromDb($"[{createdAt}] {text}");
                    }
                }
            }

            return books;
        }

        private static void InsertNewBookFeedbacks(Book book, int startFromIndex, SqlConnection conn, SqlTransaction transaction)
        {
            const string insertFeedbackSql = @"
                INSERT INTO dbo.BookFeedbacks (BookIsbn, FeedbackText)
                VALUES (@BookIsbn, @FeedbackText);";

            for (int i = startFromIndex; i < book.Feedbacks.Count; i++)
            {
                string rawFeedback = book.Feedbacks[i];

                // Odcięcie ewentualnego prefiksu "[RRRR-MM-DD GG:MM] " przy ponownym zapisie
                if (rawFeedback.StartsWith("[") && rawFeedback.Length > 19 && rawFeedback[17] == ']')
                {
                    rawFeedback = rawFeedback.Substring(19);
                }

                using var fbCmd = new SqlCommand(insertFeedbackSql, conn, transaction);
                fbCmd.Parameters.AddWithValue("@BookIsbn", book.Isbn);
                fbCmd.Parameters.AddWithValue("@FeedbackText", rawFeedback);
                fbCmd.ExecuteNonQuery();
            }
        }

        private static void LoadFeedbacksForBook(Book book, SqlConnection conn)
        {
            const string sql = @"
                SELECT CreatedAt, FeedbackText
                FROM dbo.BookFeedbacks
                WHERE BookIsbn = @BookIsbn
                ORDER BY FeedbackId ASC;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@BookIsbn", book.Isbn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string createdAt = reader.GetString(0);
                string text = reader.GetString(1);
                book.LoadFeedbackFromDb($"[{createdAt}] {text}");
            }
        }

        private static Book MapReaderToBook(SqlDataReader reader)
        {
            string isbn = reader.GetString(0);
            string title = reader.GetString(1);
            string author = reader.GetString(2);
            DateTime pubDate = reader.GetDateTime(3);
            BookStatus status = (BookStatus)reader.GetInt32(4);
            DateTime? dueDate = reader.IsDBNull(5) ? null : reader.GetDateTime(5);
            int? borrowedBy = reader.IsDBNull(6) ? null : reader.GetInt32(6);
            int? reservedBy = reader.IsDBNull(7) ? null : reader.GetInt32(7);

            return new Book(title, author, isbn, pubDate, status, dueDate, borrowedBy, reservedBy);
        }
    }
}