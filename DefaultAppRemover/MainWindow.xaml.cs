using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DefaultAppRemover
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PowerShell ps;
        const string getInstalledAppsScript = "Get-AppxPackage -AllUsers | Select Name, PackageFullName, Version, IsFramework,Architecture,InstallLocation";
        const string removeAppScript = "Get-AppxPackage ** | Remove-AppxPackage";
        public ObservableCollection<AppItem> InstalledApps { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            if (InstalledApps == null)
                InstalledApps = new ObservableCollection<AppItem>();

            ps = PowerShell.Create(RunspaceMode.NewRunspace);
            foreach (dynamic item in ExecuteScript(getInstalledAppsScript))
                InstalledApps.Add(new AppItem(item.Name,
                    item.PackageFullName, item.Version.ToString(),item.IsFramework,item.Architecture.ToString(),item.InstallLocation.ToString()));

            this.DataContext = this;
        }

        private Collection<PSObject> ExecuteScript(string script)
        {
            Collection<PSObject> results;

            Runspace runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();

            Pipeline pipeline = runspace.CreatePipeline();
            pipeline.Commands.AddScript(script);

            results = pipeline.Invoke();

            runspace.Close();


            return results;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var listItems = this.lst.Items.OfType<AppItem>().Where(x => x.Selected).ToList();
            foreach (AppItem item in listItems)
            {
                if (item.Selected)
                {
                    var script = removeAppScript.Replace("**", "*" + item.Name + "*");
                    var rs = ExecuteScript(script);
                    Console.WriteLine(item.Name);
                    InstalledApps.Remove(item);
                }
            }
        }
    }
    public class AppItem : INotifyPropertyChanged
    {
        private bool _selected;
        private string _name;
        private string _details;

        public bool Selected { get => _selected; set { _selected = value; OnChange("Selected"); } }
        public string Name { get => _name; set => _name = value; }
        public string Details { get; }
        public string Version { get; }
        public bool IsFramework { get; }
        public string Architecure { get; }
        public string InstallLocation { get; }


        public AppItem(string name, string packageName, string version,bool isFramework,string arch, string installLocation)
        {
            this.Name = name;
            this.Details = packageName;
            Version = version;
            IsFramework = isFramework;
            Architecure = arch;
            InstallLocation = installLocation;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnChange(string pname)
        {
            var c = this.PropertyChanged;
            if (c != null)
                c.Invoke(this, new PropertyChangedEventArgs(pname));
        }
    }
}
