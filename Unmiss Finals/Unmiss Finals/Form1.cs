using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Unmiss
{
    public partial class SignUp : Form
    {
        public SignUp()
        {
            InitializeComponent();
            this.AcceptButton = button1;
        }

        private string HashPassword(string password) //method used para sa hashed password
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password)); 
                return Convert.ToBase64String(hashBytes); // Return hashed password as Base64 string
            }
        }

        // Load users from the "users.txt" file into directory
        private Dictionary<string, string> LoadUsersFromFile()
        {
            var users = new Dictionary<string, string>(); // create directory para mag store ng username at password

            // Check if the file exists
            if (File.Exists("users.txt"))
            {
                var lines = File.ReadAllLines("users.txt");

                foreach (var line in lines)
                {
                    var parts = line.Split(':');
                    if (parts.Length == 2)
                    {
                        users[parts[0]] = parts[1]; // add username:hashed_password into directory
                    }
                }
            }

            return users;
        }
        private void SignUp_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtpassword.Text;

            // Load existing users from the file (this will get the dictionary with usernames and hashed passwords)
            var existingUsers = LoadUsersFromFile();

            // Check if the username exists and if the password matches (hash the entered password before comparing)
            if (existingUsers.ContainsKey(username) && existingUsers[username] == HashPassword(password)) // Hash entered password and compare
            {
                MessageBox.Show("Login successful");
                this.Hide();
                Main main = new Main(username);
                main.Show();
            }
            else
            {
                MessageBox.Show("The username or password you entered is incorrect. Try again.");
                txtUsername.Clear();
                txtpassword.Clear();
                txtUsername.Focus();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            Login login = new Login();
            login.Show();

        }
    }
}