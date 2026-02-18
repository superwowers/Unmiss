using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Unmiss
{
    public partial class Main : Form
    {
        private string currentUser; // nag store sya ng username
        private List<Assignment> assignments = new List<Assignment>(); // list ng assginment per acc

        public Main(string username)
        {
            //constructors para sa code sa baba
            InitializeComponent();  
            currentUser = username; //nag ssave ng username
            LoadAssignments();  // para sa saved assignments 
            CheckNotifications();   //nag ccheck if may notifications na ipapakita
            InitializeDataGridView();   

           
            
        }
        // para sa size ng culums and rows sa data grid
        private void InitializeDataGridView()
        {
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        }

        private void LoadAssignments() // nag lload sya ng assignment galing sa dun sa file
        {
            var filePath = GetAssignmentsFilePath();
            if (File.Exists(filePath))
            {
                TryLoadAssignments(filePath); //ni rread nya kung meron dun sa json file
            }
            else
            {
                assignments = new List<Assignment>(); //if wala sya ma read gagawa sya ng empty list
            }
            RefreshDataGridView(); //update nya yung datagrid to show assignment
        }

        private string GetAssignmentsFilePath() => Path.Combine(Application.StartupPath, $"{currentUser}_assignments.json"); // filepath para sa assignment

        private void TryLoadAssignments(string filePath) 
        {
            try
            {
                var json = File.ReadAllText(filePath); // ni rread nya laman nung file
                assignments = JsonConvert.DeserializeObject<List<Assignment>>(json) ?? new List<Assignment>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading assignments: {ex.Message}", "Error");
                assignments = new List<Assignment>();
            }
        }

        private void SaveAssignments()
        {
            try
            {
                var filePath = GetAssignmentsFilePath(); //file path
                var json = JsonConvert.SerializeObject(assignments); //convert 
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving assignments: {ex.Message}", "Error");
            }
        }

        private void RefreshDataGridView()
        {
            dataGridView1.DataSource = null;  // clear 
            dataGridView1.DataSource = assignments; // set updated list 

            if (dataGridView1.Columns.Contains("IsCompleted"))
            {
                dataGridView1.Columns["IsCompleted"].Visible = false;
            }

            dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);  
        }

        private void CheckNotifications()
        {
            foreach (var assignment in assignments)
            {
                var timeUntilNotification = (assignment.NotificationTime - DateTime.Now).TotalMinutes;

                // mag notify sya minutes before or after ng due date
                if (timeUntilNotification <= 0 && timeUntilNotification > -5)
                {
                    ShowDueDateNotification(assignment); // Show due date notification
                }

                // Check if the assignment is overdue and not completed
                if (assignment.DueDate < DateTime.Now && !assignment.IsCompleted)
                {
                    ShowIncompleteAssignmentNotification(assignment); // Show incomplete assignment notification
                }
            }
        }

        private bool ValidateAssignmentFields() // check if valid yung input
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show("Name and Description cannot be empty.", "Validation Error");
                return false;
            }

            if (dateTimePickerDueDate.Value < DateTime.Now) 
            {
                MessageBox.Show("Due date cannot be in the past.", "Validation Error");
                return false;
            }

            return true;
        }

        private void RemoveExpiredAssignments() // if past due na automatically removed
        {
            var expiredAssignments = assignments.Where(a => a.DueDate < DateTime.Now).ToList(); // check ng past due assignment
            foreach (var expiredAssignment in expiredAssignments)
            {
                assignments.Remove(expiredAssignment);
            }

            SaveAssignments(); 
            RefreshDataGridView();
        }

        private async void button1_Click(object sender, EventArgs e) // add button
        {
            if (!ValidateAssignmentFields()) return; 

            // check if okay yung time
            if (dateTimePickerNotificationTime.Value > dateTimePickerDueDate.Value)
            {
                MessageBox.Show("Notification time cannot be set after the due date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (dateTimePickerNotificationTime.Value < DateTime.Now) 
            {
                MessageBox.Show("Notification time cannot be set in the past.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var newAssignment = new Assignment // create assignment object
            {
                Id = assignments.Count + 1,
                AssignmentName = txtName.Text,
                AssignmentDescription = txtDescription.Text,
                DueDate = dateTimePickerDueDate.Value,
                NotificationTime = dateTimePickerNotificationTime.Value,
                CurrentDate = DateTime.Now
            };

            assignments.Add(newAssignment); 
            txtName.Clear();
            txtDescription.Clear();

            SaveAssignments();
            RefreshDataGridView();

            await Task.Delay(5000); // Delay for 5 seconds
            ShowDueDateNotification(newAssignment); // Show due date notification for new task
        }


        private void ShowDueDateNotification(Assignment assignment)
        {
            MessageBox.Show($"Reminder: The assignment \"{assignment.AssignmentName}\" is due on {assignment.DueDate.ToShortDateString()}. \nDescription: {assignment.AssignmentDescription}", "Due Date Reminder", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowIncompleteAssignmentNotification(Assignment assignment)
        {
            MessageBox.Show($"The assignment \"{assignment.AssignmentName}\" is past due and not completed. \nDescription: {assignment.AssignmentDescription} \nPlease make sure you complete it.", "Incomplete Assignment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
 

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var selectedAssignment = (Assignment)dataGridView1.SelectedRows[0].DataBoundItem;

                // Check if the assignment is completed
                if (!selectedAssignment.IsCompleted)
                {
                    // Ask the user if they still want to delete the uncompleted assignment
                    var result = MessageBox.Show(
                        "This assignment is not completed. Are you sure you want to delete it?",
                        "Delete Confirmation",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    // If user selects Yes, proceed with deletion
                    if (result == DialogResult.Yes)
                    {
                        DeleteSelectedAssignments();
                    }
                }
                else
                {
                    // If assignment is completed, proceed with deletion
                    DeleteSelectedAssignments();
                }
            }
            else
            {
                MessageBox.Show("Please select an assignment to delete.", "Warning");
            }
        }

        private void ReorderAssignmentIds()
        {
            // Reassign Ids starting from 1
            for (int i = 0; i < assignments.Count; i++)
            {
                assignments[i].Id = i + 1;
            }
        }

        private void DeleteSelectedAssignments()
        {
            List<Assignment> assignmentsToDelete = new List<Assignment>();

            // Collect assignments to delete in a separate list to avoid modifying the collection while iterating
            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                var selectedAssignment = (Assignment)row.DataBoundItem;

                // Prevent deletion of past-due, uncompleted assignments
                if (selectedAssignment.DueDate < DateTime.Now && !selectedAssignment.IsCompleted)
                {
                    MessageBox.Show("This assignment is past due and has not been completed. It cannot be deleted.", "Cannot Delete");
                    return;
                }

                assignmentsToDelete.Add(selectedAssignment);  // Add to deletion list
            }

            // Remove the assignments from the main list
            foreach (var assignment in assignmentsToDelete)
            {
                assignments.Remove(assignment);
            }

            // Reorder the IDs after deletion
            ReorderAssignmentIds();

            SaveAssignments();
            RefreshDataGridView();
        }

        public class Assignment
        {
            public int Id { get; set; }
            public string AssignmentName { get; set; }
            public string AssignmentDescription { get; set; }
            public DateTime DueDate { get; set; }
            public DateTime NotificationTime { get; set; }
            public DateTime CurrentDate { get; set; }
            public bool IsCompleted { get; set; }
        }

        // Event handler for Main_Load
        private void Main_Load(object sender, EventArgs e)
        {
            dateTimePickerDueDate.Format = DateTimePickerFormat.Custom;
            dateTimePickerDueDate.CustomFormat = "MMM dd, yyyy HH:mm"; // e.g., "Jan 5, 2024 08:00"
            dateTimePickerDueDate.ShowUpDown = true;

            dateTimePickerNotificationTime.Format = DateTimePickerFormat.Custom;
            dateTimePickerNotificationTime.CustomFormat = "MMM dd, yyyy HH:mm"; // e.g., "Jan 3, 2024 08:00"
            dateTimePickerNotificationTime.ShowUpDown = true;

            // Default notification time to 1 day before the due date
            dateTimePickerNotificationTime.Value = dateTimePickerDueDate.Value.AddDays(-1);

            // Initialize timer
            timer2.Interval = 60000; // 1 minute
            timer2.Tick += timer2_Tick;
            timer2.Start();

            RemoveExpiredAssignments(); // Remove expired assignments on load
        }

        // Timer tick every minute
        private void timer2_Tick(object sender, EventArgs e)
        {
            // Check for overdue assignments and show notifications
            CheckNotifications();

            RemoveExpiredAssignments(); // Clean up expired assignments
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var selectedAssignment = (Assignment)dataGridView1.SelectedRows[0].DataBoundItem;

                // Ask for confirmation before marking as completed
                var result = MessageBox.Show(
                    "Are you sure you completed this task?",
                    "Confirm Completion",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    // Mark assignment as completed
                    selectedAssignment.IsCompleted = true;

                    // Remove the completed assignment from the list
                    assignments.Remove(selectedAssignment);

                    // Refresh DataGridView to update the state
                    SaveAssignments(); // Save the updated assignments
                    RefreshDataGridView();
                }
                else
                {
                    MessageBox.Show("Task completion was not confirmed.", "Cancelled");
                }
            }
            else
            {
                MessageBox.Show("Please select an assignment to mark as completed.", "Warning");
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // If you want to handle the cell click event, write your logic here.
            // For example:
            // MessageBox.Show($"Cell clicked at row {e.RowIndex}, column {e.ColumnIndex}");
            // Otherwise, you can leave it empty or remove this event handler if not needed.
        }

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // Handle the data error here, currently suppressing exceptions.
            Console.WriteLine($"DataError occurred at row {e.RowIndex}, column {e.ColumnIndex}. Error: {e.Exception.Message}");
            e.ThrowException = false;  // Prevent the exception from propagating
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            
        }
    }
}
