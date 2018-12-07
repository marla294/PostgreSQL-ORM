using System;
using NUnit.Framework;

namespace ORM
{
    [TestFixture]
    public class PostgreSQL
    {
        PostgreSQLConnection Db { get; set; }

        public PostgreSQL()
        {
            Db = new PostgreSQLConnection();

            // Create table "test" for unit tests
            CreateTestTable();
        }

        ~PostgreSQL()
        {
            // Once tests are complete drop table "test"
            Db.DropTable("test").Execute();
        }

        void CreateTestTable()
        {
            Db.CreateTable("test", Pairing.Of("name", "text"), Pairing.Of("age", "int")).Execute();

            Db.Insert("test", Pairing.Of("name", "Marla"), Pairing.Of("age", 36)).Execute();
            Db.Insert("test", Pairing.Of("name", "Susan"), Pairing.Of("age", 100)).Execute();
            Db.Insert("test", Pairing.Of("name", "John"), Pairing.Of("age", 67)).Execute();
            Db.Insert("test", Pairing.Of("name", "Jenna"), Pairing.Of("age", 34)).Execute();
            Db.Insert("test", Pairing.Of("name", "RJ"), Pairing.Of("age", 29)).Execute();
        }

        [Test]
        public void TestCreateTable()
        {
            Db.CreateTable("newtable", Pairing.Of("name", "text"), Pairing.Of("number", "int")).Execute();

            var table = Db.Take("information_schema.tables").Where(Pairing.Of("table_name", "newtable")).Execute();
            var columns = Db.Take("information_schema.columns").Where(Pairing.Of("table_name", "newtable")).Execute()[3];

            Assert.AreEqual(1, table[0].Count);
            Assert.AreEqual(3, columns.Count);

            Db.DropTable("newtable").Execute();
        }

        [Test]
        public void TestDropTable()
        {
            Db.CreateTable("newtable", Pairing.Of("name", "text"), Pairing.Of("number", "int")).Execute();
            Db.DropTable("newtable").Execute();

            var table = Db.Take("information_schema.tables").Where(Pairing.Of("table_name", "newtable")).Execute();

            Assert.AreEqual(0, table[0].Count);
        }

        [Test]
        public void TestTake()
        {
            var results = Db.Take("test").Execute();

            Assert.IsNotNull(results);
            Assert.AreEqual("Marla", results[1][0]);
        }

        [Test]
        public void TestWhere()
        {
            var results = Db.Take("test").Where(Pairing.Of("name", "Susan"), Pairing.Of("id", 2)).Execute();

            Assert.IsNotNull(results);
            Assert.AreEqual(1, results[0].Count);
            Assert.AreEqual("Susan", results[1][0]);

            results = Db.Take("test").Where(Pairing.Of("age", 100)).Execute();

            Assert.IsNotNull(results);
            Assert.AreEqual(1, results[0].Count);
            Assert.AreEqual("Susan", results[1][0]);
        }

        [Test]
        public void TestOrderBy()
        {
            // Test default orderby
            var results = Db.Take("test").OrderBy("name").Execute();

            Assert.IsNotNull(results);
            Assert.AreEqual(5, results[0].Count);
            Assert.AreEqual("Susan", results[1][0]);

            // Test orderby ascending
            results = Db.Take("test").OrderBy("name", "asc").Execute();

            Assert.IsNotNull(results);
            Assert.AreEqual(5, results[0].Count);
            Assert.AreEqual("Jenna", results[1][0]);
        }

        [Test]
        public void TestLimit()
        {
            var results = Db.Take("test").OrderBy("name").Limit(1).Execute();

            Assert.IsNotNull(results);
            Assert.AreEqual(1, results[0].Count);
            Assert.AreEqual("Susan", results[1][0]);
        }

        [Test]
        public void TestInsert()
        {
            Db.Insert("test", Pairing.Of("name", "Bob")).Execute();

            var results = Db.Take("test").Where(Pairing.Of("name", "Bob")).Execute();

            Assert.IsNotNull(results);
            Assert.AreEqual("Bob", results[1][0]);
            Assert.AreEqual(1, results[0].Count);

            Db.Delete("test").Where(Pairing.Of("name", "Bob")).Execute();
        }

        [Test]
        public void TestUpdate()
        {
            Db.Insert("test", Pairing.Of("name", "Bob")).Execute();
            Db.Update("test", Pairing.Of("name", "Bob Update")).Where(Pairing.Of("name", "Bob")).Execute();

            var results = Db.Take("test").Where(Pairing.Of("name", "Bob Update")).Execute();

            Assert.IsNotNull(results);
            Assert.AreEqual("Bob Update", results[1][0]);
            Assert.AreEqual(1, results[0].Count);

            Db.Delete("test").Where(Pairing.Of("name", "Bob Update")).Execute();
        }

        [Test]
        public void TestDelete()
        {
            Db.Insert("test", Pairing.Of("name", "Bob")).Execute();
            Db.Delete("test").Where(Pairing.Of("name", "Bob")).Execute();

            var results1 = Db.Take("test").Where(Pairing.Of("name", "Bob")).Execute();
            var results2 = Db.Take("test").Execute();

            Assert.AreEqual(0, results1[0].Count);
            Assert.AreEqual(5, results2[0].Count);
            Assert.AreEqual("Marla", results2[1].Find(name => name == "Marla"));
            Assert.AreEqual(-1, results2[1].FindIndex(name => name == "Bob"));
        }

        [Test]
        public void TestError()
        {
            Assert.Throws<Exception>(() => { Db.Limit(1).Execute(); }, "Something went wrong with your sql statement, please try again.");
        }
    }
}
