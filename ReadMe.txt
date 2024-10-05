
This is a database helper library small but handy for dealing with MyuSQL database daily operations for which developers have write tons of lines of code.

Thje motive is that just plug and play the library for the daily DB operations in less code.

Focus on logic and not on database operations happening.

***************************************************

If you like my work PLEASE RATE THE REPO AND SPREAD THE WORD.
DO LET ME KNOW IN THE COMMENTS WHAT YOU THINK ON THIS.

***************************************************


1. Basic parameter usage:

var parameters = new Dictionary<string, DbParameter>
{
    // String parameter
    {"@name", "@name".CreateParameter("John", MySqlDbType.VarChar)},
    
    // Integer parameter
    {"@age", "@age".CreateParameter(25, MySqlDbType.Int32)},
    
    // DateTime parameter
    {"@birthDate", "@birthDate".CreateParameter(DateTime.Now, MySqlDbType.DateTime)},
    
    // Decimal parameter
    {"@salary", "@salary".CreateParameter(50000.50m, MySqlDbType.Decimal)},
    
    // Output parameter
    {"@outputParam", "@outputParam".CreateParameter(null, MySqlDbType.VarChar, ParameterDirection.Output, 100)}
};

var result = dbOps.ExecuteStoredProcedureDataTable("StoredProcedureName", parameters);



2. Using output parameters:

var parameters = new Dictionary<string, DbParameter>
{
    {"@inputParam", "@inputParam".CreateParameter("input value", MySqlDbType.VarChar)},
    {"@outputParam", "@outputParam".CreateParameter(null, MySqlDbType.VarChar, ParameterDirection.Output, 100)}
};

dbOps.ExecuteStoredProcedureScalar("StoredProcedureName", parameters);
// Access output parameter value
string outputValue = parameters["@outputParam"].Value?.ToString();



Common MySQL data types you can use:

// String types
MySqlDbType.VarChar
MySqlDbType.Text
MySqlDbType.Char

// Numeric types
MySqlDbType.Int32
MySqlDbType.Decimal
MySqlDbType.Float
MySqlDbType.Double

// Date/Time types
MySqlDbType.DateTime
MySqlDbType.Date
MySqlDbType.Time
MySqlDbType.Timestamp

// Other types
MySqlDbType.Bit
MySqlDbType.Blob
MySqlDbType.Binary