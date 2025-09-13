using CsvSmartParser;

var parser = new CsvParser();

// Parse from file
var path = Path.Combine(AppContext.BaseDirectory, "Data", "Year_End_Stock_Prices_2015_2024.csv");
var data = await parser.ParseFileAsync(path);

// Print basic information
Console.WriteLine($"Total rows: {data.Count}");
Console.WriteLine();

// Print all data
foreach (var row in data)
{
    foreach (var kvp in row)
    {
        Console.Write($"{kvp.Key}: {kvp.Value} | ");
    }
    Console.WriteLine();
}



// Parse from string
var csvString = "Name,Age,City\nJohn,30,New York\nJane,25,Los Angeles";
var result = await parser.ParseStringAsync(csvString);

foreach (var row in result)
{
    Console.WriteLine($"Name: {row["Name"]}, Age: {row["Age"]}, City: {row["City"]}");
}
