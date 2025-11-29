namespace PlainFiles.Core;

public class UserManager
{
    private readonly string _usersFilePath;
    private List<User> _users;

    public UserManager(string usersFilePath)
    {
        _usersFilePath = usersFilePath;
        LoadUsers();
    }

    private void LoadUsers()
    {
        _users = new List<User>();

        if (!File.Exists(_usersFilePath))
        {
            throw new FileNotFoundException($"El archivo de usuarios no existe: {_usersFilePath}");
        }

        var lines = File.ReadAllLines(_usersFilePath);
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split(',');
            if (parts.Length == 3)
            {
                _users.Add(new User
                {
                    Username = parts[0].Trim(),
                    Password = parts[1].Trim(),
                    IsActive = bool.Parse(parts[2].Trim())
                });
            }
        }
    }

    public bool UserExists(string username)
    {
        return _users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    public (bool success, string message) Authenticate(string username, string password)
    {
        var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (user == null)
        {
            return (false, "Usuario no encontrado");
        }

        if (!user.IsActive)
        {
            return (false, "Usuario bloqueado. Contacte al administrador");
        }

        if (user.Password != password)
        {
            return (false, "Contraseña incorrecta");
        }

        return (true, "Autenticación exitosa");
    }

    public void BlockUser(string username)
    {
        var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        if (user != null)
        {
            user.IsActive = false;
            SaveUsers();
        }
    }

    public void UnblockUser(string username)
    {
        var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        if (user != null)
        {
            user.IsActive = true;
            SaveUsers();
        }
    }

    public List<User> GetAllUsers()
    {
        return _users.ToList();
    }

    private void SaveUsers()
    {
        var lines = _users.Select(u => $"{u.Username},{u.Password},{u.IsActive}");
        File.WriteAllLines(_usersFilePath, lines);
    }
}