using PlainFiles.Core;

string usersFilePath = "Users.txt";
string logFilePath = "log.txt";
string peopleFilePath = "people.csv";

var userManager = new UserManager(usersFilePath);
LogWriter? logWriter = null;

string loggedInUser = Login(userManager);

if (string.IsNullOrEmpty(loggedInUser))
{
    Console.WriteLine("\nAcceso denegado. El programa se cerrará.");
    Thread.Sleep(1500);
    return;
}

logWriter = new LogWriter(logFilePath);
logWriter.WriteLog("INFO", "Inicio de sesión exitoso", loggedInUser);

Console.Clear();
Console.WriteLine($"¡Bienvenido {loggedInUser}!");
Thread.Sleep(1000);

var helper = new NugetCsvHelper();
var people = new List<Person>();

if (File.Exists(peopleFilePath))
{
    try
    {
        people = helper.Read(peopleFilePath).ToList();
        logWriter.WriteLog("INFO", $"Archivo cargado: {people.Count} registros", loggedInUser);
    }
    catch (Exception ex)
    {
        logWriter.WriteLog("ERROR", $"Error al cargar archivo: {ex.Message}", loggedInUser);
        Thread.Sleep(2000);
    }
}

string option;
do
{
    option = ShowMenu();

    switch (option)
    {
        case "1":
            AddPerson();
            break;

        case "2":
            ListPeople();
            break;

        case "3":
            EditPerson();
            break;

        case "4":
            DeletePerson();
            break;

        case "5":
            ShowCityReport();
            break;

        case "6":
            SaveData();
            break;

        case "0":
            logWriter.WriteLog("INFO", "Cierre de sesión", loggedInUser);
            Console.WriteLine("Saliendo del sistema...");
            break;

        default:
            Console.WriteLine("❌ Opción no válida");
            Thread.Sleep(1000);
            break;
    }
} while (option != "0");

SaveData();
logWriter?.Dispose();
Console.WriteLine("¡Hasta pronto!");
Thread.Sleep(1000);

string Login(UserManager manager)
{
    int attempts = 0;
    const int maxAttempts = 3;
    string username = "";

    while (attempts < maxAttempts)
    {
        Console.Clear();
        Console.WriteLine("╔═══════════════════════════════════════╗");
        Console.WriteLine("║     SISTEMA DE AUTENTICACIÓN          ║");
        Console.WriteLine("╚═══════════════════════════════════════╝");
        Console.WriteLine($"\nIntento {attempts + 1} de {maxAttempts}\n");

        Console.Write("Usuario: ");
        username = Console.ReadLine()?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(username))
        {
            Console.WriteLine("\n❌ El usuario no puede estar vacío");
            attempts++;
            Thread.Sleep(1500);
            continue;
        }

        // VERIFICAR SI EL USUARIO EXISTE
        if (!manager.UserExists(username))
        {
            Console.WriteLine("\n❌ El usuario no existe en el sistema");
            attempts++;
            Thread.Sleep(1500);
            continue;
        }

        Console.Write("Contraseña: ");
        string password = ReadPassword();

        var (success, message) = manager.Authenticate(username, password);

        if (success)
        {
            Console.WriteLine("\n✓ Autenticación exitosa");
            Thread.Sleep(1000);
            return username;
        }

        Console.WriteLine($"\n❌ {message}");
        attempts++;
        Thread.Sleep(1500);
    }

    Console.WriteLine($"\n⚠️  Usuario '{username}' bloqueado por intentos fallidos");
    manager.BlockUser(username);
    Thread.Sleep(2000);

    return "";
}

string ReadPassword()
{
    string password = "";
    ConsoleKeyInfo key;

    do
    {
        key = Console.ReadKey(true);

        if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
        {
            password += key.KeyChar;
            Console.Write("*");
        }
        else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
        {
            password = password[..^1];
            Console.Write("\b \b");
        }
    }
    while (key.Key != ConsoleKey.Enter);

    Console.WriteLine();
    return password;
}

string ShowMenu()
{
    Console.Clear();
    Console.WriteLine("╔═══════════════════════════════════════╗");
    Console.WriteLine($"║  Usuario: {loggedInUser,-27}        ║");
    Console.WriteLine("╠═══════════════════════════════════════╣");
    Console.WriteLine("║        GESTIÓN DE PERSONAS            ║");
    Console.WriteLine("╠═══════════════════════════════════════╣");
    Console.WriteLine("║  1. Adicionar Persona                 ║");
    Console.WriteLine("║  2. Listar Personas                   ║");
    Console.WriteLine("║  3. Editar Persona                    ║");
    Console.WriteLine("║  4. Eliminar Persona                  ║");
    Console.WriteLine("║  5. Informe por Ciudad                ║");
    Console.WriteLine("║  6. Guardar Cambios                   ║");
    Console.WriteLine("║  0. Salir                             ║");
    Console.WriteLine("╚═══════════════════════════════════════╝");
    Console.Write("\nOpción: ");
    return Console.ReadLine()?.Trim() ?? "";
}

void AddPerson()
{
    Console.Clear();
    Console.WriteLine("═══ ADICIONAR PERSONA ═══\n");

    string id = "";
    while (true)
    {
        Console.Write("ID (número único): ");
        id = Console.ReadLine()?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(id))
        {
            Console.WriteLine("❌ El ID no puede estar vacío");
            continue;
        }

        if (!int.TryParse(id, out _))
        {
            Console.WriteLine("❌ El ID debe ser un número");
            continue;
        }

        if (people.Any(p => p.Id == id))
        {
            Console.WriteLine("❌ El ID ya existe");
            continue;
        }

        break;
    }

    string firstName = "";
    while (string.IsNullOrWhiteSpace(firstName))
    {
        Console.Write("Nombres: ");
        firstName = Console.ReadLine()?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(firstName))
            Console.WriteLine("❌ Los nombres son obligatorios");
    }

    string lastName = "";
    while (string.IsNullOrWhiteSpace(lastName))
    {
        Console.Write("Apellidos: ");
        lastName = Console.ReadLine()?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(lastName))
            Console.WriteLine("❌ Los apellidos son obligatorios");
    }

    string phone = "";
    while (true)
    {
        Console.Write("Teléfono: ");
        phone = Console.ReadLine()?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(phone))
        {
            Console.WriteLine("❌ El teléfono no puede estar vacío");
            continue;
        }

        int digitCount = phone.Count(char.IsDigit);
        if (digitCount < 7)
        {
            Console.WriteLine("❌ El teléfono debe tener al menos 7 dígitos");
            continue;
        }

        break;
    }

    Console.Write("Ciudad: ");
    string city = Console.ReadLine()?.Trim() ?? "";

    decimal balance = 0;
    while (true)
    {
        Console.Write("Saldo: ");
        string input = Console.ReadLine()?.Trim() ?? "";

        if (!decimal.TryParse(input, out balance))
        {
            Console.WriteLine("❌ El saldo debe ser un número válido");
            continue;
        }

        if (balance < 0)
        {
            Console.WriteLine("❌ El saldo debe ser positivo");
            continue;
        }

        break;
    }

    var person = new Person
    {
        Id = id,
        FirstName = firstName,
        LastName = lastName,
        Phone = phone,
        City = city,
        Balance = balance
    };

    people.Add(person);
    Console.WriteLine("\n✓ Persona agregada exitosamente");
    logWriter.WriteLog("INFO", $"Persona agregada - ID: {id}, Nombre: {firstName} {lastName}", loggedInUser);
    Thread.Sleep(1500);
}

void ListPeople()
{
    Console.Clear();
    Console.WriteLine("═══ LISTA DE PERSONAS ═══\n");

    if (people.Count == 0)
    {
        Console.WriteLine("No hay personas registradas");
        logWriter.WriteLog("INFO", "Consultó lista vacía", loggedInUser);
        Thread.Sleep(1500);
        return;
    }

    Console.WriteLine($"{"ID",-6} {"Nombres",-20} {"Apellidos",-20} {"Teléfono",-15} {"Ciudad",-15} {"Saldo",12}");
    Console.WriteLine(new string('─', 95));

    foreach (var p in people)
    {
        Console.WriteLine($"{p.Id,-6} {p.FirstName,-20} {p.LastName,-20} {p.Phone,-15} {p.City,-15} {p.Balance,12:C}");
    }

    Console.WriteLine($"\nTotal: {people.Count} persona(s)");
    logWriter.WriteLog("INFO", $"Consultó lista de {people.Count} persona(s)", loggedInUser);

    Console.WriteLine("\nPresione cualquier tecla para volver al menú...");
    Console.ReadKey();
}

void EditPerson()
{
    Console.Clear();
    Console.WriteLine("═══ EDITAR PERSONA ═══\n");

    if (people.Count == 0)
    {
        Console.WriteLine("No hay personas registradas");
        Thread.Sleep(1500);
        return;
    }
    Console.Write("Ingrese ID de la persona: ");
    string id = Console.ReadLine()?.Trim() ?? "";

    var person = people.FirstOrDefault(p => p.Id == id);

    if (person == null)
    {
        Console.WriteLine("❌ Persona no encontrada");
        logWriter.WriteLog("WARNING", $"Intento editar ID inexistente: {id}", loggedInUser);
        Thread.Sleep(1500);
        return;
    }

    Console.WriteLine("\n--- Datos Actuales ---");
    Console.WriteLine($"Nombres: {person.FirstName}");
    Console.WriteLine($"Apellidos: {person.LastName}");
    Console.WriteLine($"Teléfono: {person.Phone}");
    Console.WriteLine($"Ciudad: {person.City}");
    Console.WriteLine($"Saldo: {person.Balance:C}");
    Console.WriteLine("\n(Presione ENTER para mantener el valor actual)\n");

    Console.Write($"Nombres [{person.FirstName}]: ");
    string input = Console.ReadLine()?.Trim() ?? "";
    if (!string.IsNullOrWhiteSpace(input))
        person.FirstName = input;

    Console.Write($"Apellidos [{person.LastName}]: ");
    input = Console.ReadLine()?.Trim() ?? "";
    if (!string.IsNullOrWhiteSpace(input))
        person.LastName = input;

    while (true)
    {
        Console.Write($"Teléfono [{person.Phone}]: ");
        input = Console.ReadLine()?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(input))
            break;

        int digitCount = input.Count(char.IsDigit);
        if (digitCount < 7)
        {
            Console.WriteLine("❌ El teléfono debe tener al menos 7 dígitos");
            continue;
        }

        person.Phone = input;
        break;
    }

    Console.Write($"Ciudad [{person.City}]: ");
    input = Console.ReadLine()?.Trim() ?? "";
    if (!string.IsNullOrWhiteSpace(input))
        person.City = input;

    while (true)
    {
        Console.Write($"Saldo [{person.Balance:C}]: ");
        input = Console.ReadLine()?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(input))
            break;

        if (!decimal.TryParse(input, out decimal balance))
        {
            Console.WriteLine("❌ El saldo debe ser un número válido");
            continue;
        }

        if (balance < 0)
        {
            Console.WriteLine("❌ El saldo debe ser positivo");
            continue;
        }

        person.Balance = balance;
        break;
    }

    Console.WriteLine("\n✓ Persona actualizada exitosamente");
    logWriter.WriteLog("INFO", $"Persona editada - ID: {id}", loggedInUser);
    Thread.Sleep(1500);
}

void DeletePerson()
{
    Console.Clear();
    Console.WriteLine("═══ ELIMINAR PERSONA ═══\n");

    if (people.Count == 0)
    {
        Console.WriteLine("No hay personas registradas");
        Thread.Sleep(1500);
        return;
    }

    Console.Write("Ingrese ID de la persona: ");
    string id = Console.ReadLine()?.Trim() ?? "";

    var person = people.FirstOrDefault(p => p.Id == id);

    if (person == null)
    {
        Console.WriteLine("❌ Persona no encontrada");
        logWriter.WriteLog("WARNING", $"Intento eliminar ID inexistente: {id}", loggedInUser);
        Thread.Sleep(1500);
        return;
    }

    Console.WriteLine("\n--- Datos de la Persona ---");
    Console.WriteLine($"ID: {person.Id}");
    Console.WriteLine($"Nombres: {person.FirstName}");
    Console.WriteLine($"Apellidos: {person.LastName}");
    Console.WriteLine($"Teléfono: {person.Phone}");
    Console.WriteLine($"Ciudad: {person.City}");
    Console.WriteLine($"Saldo: {person.Balance:C}");

    Console.Write("\n¿Confirma la eliminación? (S/N): ");
    string confirm = Console.ReadLine()?.ToUpper().Trim() ?? "";

    if (confirm == "S" || confirm == "SI")
    {
        people.Remove(person);
        Console.WriteLine("\n✓ Persona eliminada exitosamente");
        logWriter.WriteLog("INFO", $"Persona eliminada - ID: {id}, Nombre: {person.FirstName} {person.LastName}", loggedInUser);
    }
    else
    {
        Console.WriteLine("\n✓ Operación cancelada");
        logWriter.WriteLog("INFO", $"Canceló eliminación de ID: {id}", loggedInUser);
    }
    Thread.Sleep(1500);
}

void ShowCityReport()
{
    Console.Clear();
    Console.WriteLine("═══ INFORME POR CIUDAD ═══\n");

    if (people.Count == 0)
    {
        Console.WriteLine("No hay personas registradas");
        logWriter.WriteLog("INFO", "Consultó informe vacío", loggedInUser);
        Thread.Sleep(1500);
        return;
    }

    var grouped = people
        .GroupBy(p => string.IsNullOrWhiteSpace(p.City) ? "Sin Ciudad" : p.City)
        .OrderBy(g => g.Key);

    decimal totalGeneral = 0;

    foreach (var group in grouped)
    {
        Console.WriteLine($"\nCiudad: {group.Key}");
        Console.WriteLine($"{"ID",-6} {"Nombres",-20} {"Apellidos",-20} {"Saldo",15}");
        Console.WriteLine(new string('─', 68));

        decimal subtotal = 0;

        foreach (var p in group)
        {
            Console.WriteLine($"{p.Id,-6} {p.FirstName,-20} {p.LastName,-20} {p.Balance,15:N2}");
            subtotal += p.Balance;
        }

        Console.WriteLine(new string(' ', 53) + new string('═', 15));
        Console.WriteLine($"{"Total: " + group.Key,53} {subtotal,15:N2}");

        totalGeneral += subtotal;
    }

    Console.WriteLine("\n" + new string(' ', 53) + new string('═', 15));
    Console.WriteLine($"{"Total General:",53} {totalGeneral,15:N2}");

    logWriter.WriteLog("INFO", $"Consultó informe por ciudad - Total: {totalGeneral:C}", loggedInUser);

    Console.WriteLine("\nPresione cualquier tecla para volver al menú...");
    Console.ReadKey();
}

void SaveData()
{
    try
    {
        helper.Write(peopleFilePath, people);
        Console.WriteLine("✓ Datos guardados exitosamente");
        logWriter.WriteLog("INFO", $"Datos guardados - {people.Count} registro(s)", loggedInUser);
        Thread.Sleep(1000);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error al guardar: {ex.Message}");
        logWriter.WriteLog("ERROR", $"Error al guardar: {ex.Message}", loggedInUser);
        Thread.Sleep(2000);
    }
}