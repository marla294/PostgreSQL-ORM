# PostgreSQL-ORM
Connecting .NET -> PostgreSQL

## What is this thing?

This is a custom ORM for use with .NET and PostgreSQL that I made for a project.  I don't need all the full-blown features of an ORM in that project, so I am moving it into its own repo here, so that other people can just use this on its own and I can go crazy adding all the features I want to it.

All of the queries use parameters when necessary and also using prepared commands for performance improvements.

## How do you use it?

There are 6 basic methods:

* Take(string table)
* Update(string table, KeyValuePair<string, object> setValue)
* Insert(string table, KeyValuePair<string, object>[] insertValues)
* Delete(string table)
* CreateTable(string table, params KeyValuePair<string, object>[] columnTypes)
* DropTable(string table)

You have to use one of those methods first on the PostgreSQLConnection object.  Then if you want, you can juice your queries/commands up with one of the other methods:

* Where(KeyValuePair<string, object>[] whereValues)
* OrderBy(string orderBy, string orderByDirection = "desc")
* Limit(int limit)

When you're ready to execute the command, tack Execute() on the end.  If it is a query (anything using Take at the beginning), you will get a result in the format List<List<string>>, otherwise the command will simply execute.

## Give us some examples, please!

Alright, fine.

* List<List<string>> result = db.Take("tests").Where(Pairing.Of("name", "Joe Schmoe")).Limit(1).Execute();
  * Returns the top row in the table "tests" with name = "Joe Schmoe" in the database.

## Unit tests

There are unit tests set up in NUnit for this but currently they are based off my internal db.  I intend to create a function that will add a "test" table to your database to use for the unit tests.  If you don't like that for some reason, feel free to comment it out.

## To do

* ~Add function to create table for using for unit tests~
* Make error handling more robust (rn barely catching anything)
* Make Where accept a list of strings so you can do "where column like 'whatever'" and stuff like that
* Make an "Or" function for use with Where so you can use Or instead of and - where column = 'whatever' or column = 'anything'"
* Add joins
