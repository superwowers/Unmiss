using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;

namespace Unmiss
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
            this.AcceptButton = button2;
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create()) 
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password)); 
                return Convert.ToBase64String(hashBytes); // Return hashed password as Base64 string
            }
        }

        // Load existing users from the file
        private Dictionary<string, string> LoadUsersFromFile()
        {
            var users = new Dictionary<string, string>();

            // Check if the file exists
            if (File.Exists("users.txt"))
            {
                var lines = File.ReadAllLines("users.txt");

                foreach (var line in lines)
                {
                    var parts = line.Split(':');
                    if (parts.Length == 2)
                    {
                        users[parts[0]] = parts[1]; // mag add ng username:hashed_password sa directory
                    }
                }
            }

            return users;
        }

        // method to save the users to the file
        private void SaveUsersToFile(Dictionary<string, string> users)
        {
            var lines = users.Select(user => $"{user.Key}:{user.Value}").ToArray(); 
            File.WriteAllLines("users.txt", lines);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            SignUp signup = new SignUp();
            signup.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            // Check if yung password at confirm password ay magkatulad
            if (password != confirmPassword)
            {
                MessageBox.Show("The passwords do not match. Please try again.");
                return;
            }

            // Check if meron na katulad na username
            var existingUsers = LoadUsersFromFile();

            if (existingUsers.ContainsKey(username))
            {
                MessageBox.Show("Username already exists. Please choose a different username.");
            }
            else
            {
                // Hash the password 
                string hashedPassword = HashPassword(password);

                // mag add sya ng new username and password dun sa dictionary
                existingUsers[username] = hashedPassword;

                // I save nya yung updated list of users
                SaveUsersToFile(existingUsers);

                MessageBox.Show("Sign Up successful! You can now log in.");
            }

            // Clear fields
            txtUsername.Clear();
            txtPassword.Clear();
            txtConfirmPassword.Clear();
        }

        private void Login_Load(object sender, EventArgs e)
        {

        }
    }
}


