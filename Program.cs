using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static void Main()
    {
        // Name of the text file where encrypted entries are saved
        string fileName = "diary.txt";

        // Boolean flag to control the main menu loop execution
        bool keepRunning = true;

        // Counter for tracking invalid login attempts
        int attempts = 0;

        // Maximum allowed failed login attempts before triggering lockout
        const int maxAttempts = 3;

        // ===================================================
        // LOGIN & AUTHENTICATION SYSTEM
        // ===================================================
        while (attempts < maxAttempts)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== 🛡️ SECURE DIARY LOGIN ===");
            Console.ResetColor();

            Console.Write($"Enter 4-digit PIN (Attempt {attempts + 1}/{maxAttempts}): ");
            string pin = "";

            // Custom loop to read character-by-character and hide input with asterisks
            while (true)
            {
                // Console.ReadKey(true) intercepts the key press without printing it to the console
                ConsoleKeyInfo key = Console.ReadKey(true);

                // Stop reading input when the user presses Enter
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                // Handle Backspace: remove last character from pin string and erase asterisk from screen
                else if (key.Key == ConsoleKey.Backspace && pin.Length > 0)
                {
                    pin = pin.Substring(0, pin.Length - 1);
                    Console.Write("\b \b"); // Moves cursor back, writes space to erase, moves back again
                }
                // Ignore special control keys and append valid characters
                else if (!char.IsControl(key.KeyChar))
                {
                    pin += key.KeyChar;
                    Console.Write("*"); // Print asterisk mask for privacy
                }
            }

            // Verify if the entered PIN matches the hardcoded password ("1234")
            if (pin == "1234")
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ Access Granted! Welcome back.");
                Console.ResetColor();
                System.Threading.Thread.Sleep(1000); // Pause briefly before loading the main menu
                break; // Break out of the authentication loop to proceed
            }
            else
            {
                attempts++;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Wrong PIN! ({maxAttempts - attempts} attempts left)");
                Console.ResetColor();
                System.Threading.Thread.Sleep(1500);

                // Anti-brute-force mechanism: lock the application for 10 seconds if attempts reach limit
                if (attempts == maxAttempts)
                {
                    Console.WriteLine("\nToo many failed attempts. App is locking for 10 seconds...");
                    for (int i = 10; i > 0; i--)
                    {
                        Console.Write($"\rLocked: {i}s remaining... "); // \r moves cursor to line start for countdown update
                        System.Threading.Thread.Sleep(1000);
                    }
                    attempts = 0; // Reset failed attempts counter after lockout finishes
                }
            }
        }

        // ===================================================
        // MAIN APPLICATION MENU LOOP
        // ===================================================
        while (keepRunning)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== 📓 MY DIGITAL DIARY ===");
            Console.ResetColor();
            Console.WriteLine("1. Write a new entry");
            Console.WriteLine("2. Read all entries");
            Console.WriteLine("3. Search entries by keyword");
            Console.WriteLine("4. Delete an entry");
            Console.WriteLine("5. View Statistics");
            Console.WriteLine("6. Exit");
            Console.Write("\nChoose an option: ");

            // Read user choice; use null-coalescing operator (?? "") to prevent null references
            string choice = Console.ReadLine() ?? "";

            // Dispatch execution to appropriate method based on menu option selected
            switch (choice)
            {
                case "1":
                    WriteEntry(fileName);
                    break;
                case "2":
                    ReadEntries(fileName);
                    break;
                case "3":
                    SearchEntries(fileName);
                    break;
                case "4":
                    DeleteEntry(fileName);
                    break;
                case "5":
                    ShowStatistics(fileName);
                    break;
                case "6":
                    keepRunning = false; // Set flag to false to break loop and end program
                    Console.WriteLine("Goodbye! Have a great day.");
                    System.Threading.Thread.Sleep(1000);
                    break;
                default:
                    Console.WriteLine("Invalid choice. Press any key to try again...");
                    Console.ReadKey();
                    break;
            }
        }
    }

    // ===================================================
    // FEATURE 1: WRITE NEW ENTRY
    // ===================================================
    // Prompts user for diary text and mood, encrypts data, and appends it to file
    static void WriteEntry(string file)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("--- New Diary Entry ---");
        Console.ResetColor();

        Console.WriteLine("What's on your mind?");
        string content = Console.ReadLine() ?? "";

        // Defensive input validation loop: enforces mood rating strictly between 1 and 5
        int mood = 0;
        while (mood < 1 || mood > 5)
        {
            Console.Write("Rate your mood (1: Terrible, 5: Amazing): ");
            // int.TryParse safely converts string input to integer without throwing exceptions
            if (!int.TryParse(Console.ReadLine(), out mood) || mood < 1 || mood > 5)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Please enter a number between 1 and 5.");
                Console.ResetColor();
            }
        }

        // Format entry string with timestamp header, mood score, and user content
        string entry = $"[{DateTime.Now:dd/MM/yyyy HH:mm}] | Mood: {mood} | {content}";

        // Encrypt the plain text string using Caesar cipher shift key = 5
        string encryptedEntry = Encrypt(entry, 5); 

        // Append encrypted entry as a new line to storage file (creates file if it doesn't exist)
        File.AppendAllLines(file, new List<string> { encryptedEntry });

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nSaved! Your mood has been recorded. Press any key...");
        Console.ResetColor();
        Console.ReadKey();
    }

    // ===================================================
    // FEATURE 2: READ ALL ENTRIES
    // ===================================================
    // Reads all encrypted records, decrypts them in memory, and prints with mood coloring
    static void ReadEntries(string file)
    {
        Console.Clear();
        Console.WriteLine("--- 📓 YOUR HISTORY (Decrypted) ---\n");

        // Check if the diary file exists on disk before attempting to read
        if (File.Exists(file))
        {
            // Read every line from the file into a string array
            string[] encryptedLines = File.ReadAllLines(file);

            // Iterate over every line, decrypt it, and print with dynamic text color
            foreach (var line in encryptedLines)
            {
                string decryptedLine = Decrypt(line, 5);
                PrintWithMoodColor(decryptedLine);
            }
        }
        else 
        { 
            Console.WriteLine("No entries found."); 
        }

        Console.WriteLine("\nPress any key to return...");
        Console.ReadKey();
    }

    // ===================================================
    // FEATURE 3: DELETE ENTRY
    // ===================================================
    // Lists entries with index numbers and lets user choose an entry to delete
    static void DeleteEntry(string file)
    {
        Console.Clear();
        Console.WriteLine("--- 🗑️ DELETE AN ENTRY ---");

        // Guard clause: return early if storage file does not exist
        if (!File.Exists(file)) { Console.WriteLine("No entries found."); return; }

        // Load all lines into a List to allow dynamic element removal
        List<string> encryptedLines = File.ReadAllLines(file).ToList();
    
        if (encryptedLines.Count == 0) { Console.WriteLine("Diary is empty."); return; }

        // Loop through and display each entry with a 1-based index (1, 2, 3...)
        for (int i = 0; i < encryptedLines.Count; i++)
        {
            string decLine = Decrypt(encryptedLines[i], 5);
            Console.Write($"{i + 1}. ");
            PrintWithMoodColor(decLine); 
        }

        Console.Write("\nGive entry number to delete (0 to cancel): ");
        // Validate user input index against valid list range
        if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= encryptedLines.Count)
        {
            Console.Write($"Are you sure you want to delete entry #{index}? (y/n): ");
            if ((Console.ReadLine() ?? "").ToLower() == "y")
            {
                // Remove selected entry from memory list (index - 1 converts 1-based to 0-based)
                encryptedLines.RemoveAt(index - 1); 

                // Overwrite file with remaining entries list
                File.WriteAllLines(file, encryptedLines); 
                Console.WriteLine("✅ Entry deleted successfully.");
            }
        }

        Console.ReadKey();
    }

    // ===================================================
    // FEATURE 4: SEARCH ENTRIES BY KEYWORD
    // ===================================================
    // Decrypts entries and checks for matching substring keywords (case-insensitive)
    static void SearchEntries(string file)
    {
        Console.Clear();
        Console.WriteLine("--- 🔍 SEARCH ENTRIES ---");
        Console.Write("Enter keyword to find: ");

        // Convert search term to lowercase to support case-insensitive matching
        string keyword = (Console.ReadLine() ?? "").ToLower();

        if (File.Exists(file))
        {
            string[] encryptedLines = File.ReadAllLines(file);
            bool found = false;

            foreach (var encLine in encryptedLines)
            {
                // Decrypt line first before searching keyword
                string decLine = Decrypt(encLine, 5);
            
                // Perform case-insensitive substring search
                if (decLine.ToLower().Contains(keyword))
                {
                    PrintWithMoodColor(decLine);
                    found = true; // Flag that at least one match was found
                }
            }

            if (!found) Console.WriteLine("❌ No matches found for your keyword.");
        }
        else { Console.WriteLine("Diary file not found."); }
    
        Console.WriteLine("\nPress any key...");
        Console.ReadKey();
    }

    // ===================================================
    // FEATURE 5: VIEW STATISTICS & ANALYTICS
    // ===================================================
    // Parses diary records to calculate entry count, total words, and average mood rating
    static void ShowStatistics(string file)
    {
        Console.Clear();
        Console.BackgroundColor = ConsoleColor.DarkBlue;
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("=== 📊 DIARY STATISTICS ===");
        Console.ResetColor();

        if (File.Exists(file))
        {
            var lines = File.ReadAllLines(file);
            double totalMood = 0;
            int moodCount = 0;
            int totalWords = 0;

            foreach (var encLine in lines)
            {   
                string line = Decrypt(encLine, 5);

                // Count words by splitting string on spaces and discarding empty strings
                totalWords += line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

                // Extract numeric mood value from the formatted entry string ("Mood: X")
                if (line.Contains("Mood: "))
                {
                    int index = line.IndexOf("Mood: ") + 6; // Locate index where mood digit starts
                    if (int.TryParse(line.Substring(index, 1), out int val))
                    {
                        totalMood += val; // Accumulate sum of mood scores
                        moodCount++;      // Increment valid mood entries counter
                    }
                }
            }

            Console.WriteLine($"\nTotal Entries: {lines.Length}");
            Console.WriteLine($"Total Words Written: {totalWords}");

            // Compute average mood score and display with threshold-based color coding
            if (moodCount > 0)
            {
                double average = totalMood / moodCount;
                Console.Write("Average Mood Score: ");

                // Color code result: Green for high average, Yellow for medium, Red for low
                if (average >= 4) Console.ForegroundColor = ConsoleColor.Green;
                else if (average >= 2.5) Console.ForegroundColor = ConsoleColor.Yellow;
                else Console.ForegroundColor = ConsoleColor.Red;
    
                Console.WriteLine($"{average:F1} / 5.0"); // Format to 1 decimal place
                Console.ResetColor();
            }
        }
        else
        {
            Console.WriteLine("\nNo diary file found. Create your first entry!");
        }

        Console.WriteLine("\nPress any key to return...");
        Console.ReadKey();
    }

    // ===================================================
    // HELPER UTILITIES: ENCRYPTION & FORMATTING
    // ===================================================

    // Encrypts text using a basic Caesar cipher (shifts ASCII character values forward by key)
    static string Encrypt(string text, int key)
    {
        char[] buffer = text.ToCharArray();
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (char)(buffer[i] + key); // Shift character code
        }
        return new string(buffer);
    }

    // Decrypts text by reversing the Caesar cipher shift (shifts ASCII character values back by key)
    static string Decrypt(string text, int key)
    {
        char[] buffer = text.ToCharArray();
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (char)(buffer[i] - key); // Reverse shift character code
        }
        return new string(buffer);
    }

    // Reads mood score from string and applies corresponding console text color
    static void PrintWithMoodColor(string line)
    {
        if (line.Contains("Mood: 5")) Console.ForegroundColor = ConsoleColor.Green;
        else if (line.Contains("Mood: 4")) Console.ForegroundColor = ConsoleColor.Cyan;
        else if (line.Contains("Mood: 1") || line.Contains("Mood: 2")) Console.ForegroundColor = ConsoleColor.Red;
        else Console.ForegroundColor = ConsoleColor.White;

        Console.WriteLine(line);
        Console.ResetColor(); // Always reset color back to default
    }
}