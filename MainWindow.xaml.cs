using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Security.Policy;

namespace ZatBlagTestWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {



        SqlConnection sqlConnection;

        //Konekcija na SQl server preko serverskog korisnika
        public const string connectionString = "Data Source=SERVERNAME;Initial Catalog=DBNAME;User Id=USERNAME;Password=PASSWORD";


        public MainWindow()
        {
            InitializeComponent();


        }

        //Validation da se unose samo brojevi u textbox
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);


        }



        //Dugme PROVERA
        private void Provera_Click(object sender, RoutedEventArgs e)
        {
            //Ovo koristimo za proveru izabranog datuma
            DateTime now = DateTime.Now;


            //provera da li je dobro uneta poslovnica
            if (!CheckPoslInput(txtPoslovnica.Text))
            {
                MessageBox.Show("Pogresno unet broj poslovnice");
                txtPoslovnica.Focus();
            }
            else
            {
                //Provera da li je datum izabran
                if (dtDatum.SelectedDate == null || dtDatum.SelectedDate >= DateTime.Today)
                {
                    MessageBox.Show("Morate da izaberete datum i da bude pre danasnjeg");
                    dtDatum.Focus();
                }
                else
                {
                    //Izabrani datum ne sme da bude stariji od 3 meseca
                    if (dtDatum.SelectedDate < now.AddDays(-8))
                    {
                        MessageBox.Show("Izabrani datum ne sme da bude stariji od 7 dana");
                        dtDatum.Focus();
                    }
                    else
                    {
                        //da li je dobro uneta blagajna
                        if (!CheckBlagInput(txtBlag.Text))
                        {
                            MessageBox.Show("Blagajna nije uneta kako treba");
                            txtBlag.Focus();
                        }
                        else
                        {
                            //Ako je sve uneto ok
                            if (CheckPoslInput(txtPoslovnica.Text) && dtDatum.SelectedDate != null && dtDatum.SelectedDate < DateTime.Today && dtDatum.SelectedDate > now.AddDays(-8) && CheckBlagInput(txtBlag.Text))
                            {
                                //pre pokretanja upita ka serveru pingujemo ga
                                if (PingServer("192.168.1.46"))
                                {
                                    //ako uneta poslovnica ne postoji u bazi
                                    if (!ProveraPosl())
                                    {
                                        MessageBox.Show("Poslovnica ne postoji u bazi");
                                        txtPoslovnica.Focus();
                                    }
                                    //Poslovnica postoji u bazi
                                    else
                                    {
                                        //ako blagajna ne postoji u bazi ili u toj poslovnici
                                        if (!ProveraBlag())
                                        {
                                            MessageBox.Show("Blagajna ne postoji u bazi ili u toj poslovnici");
                                            txtBlag.Focus();
                                        }
                                        //ako je i blagajna i poslovnica u bazi
                                        else
                                        {
                                            DateTime datum = (DateTime)dtDatum.SelectedDate;
                                            //ovo je konverzija da vidim kako to treba da izgleda za prebacivanje u bazu
                                            string strDatum = datum.ToString("yyyyMMdd");

                                            string? ipKase = GetBlagIP();

                                            if (ipKase == null)
                                            {
                                                MessageBox.Show("IP BLAG JE NULL", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                                            }
                                            else
                                            {
                                                //ako ne pingujemo kasu
                                                if (!PingServer(ipKase))
                                                {
                                                    MessageBox.Show("Ne pingujem kasu!", "INFO", MessageBoxButton.OK, MessageBoxImage.Warning);
                                                    lbResult.Content = "Ne pingujem kasu!";
                                                    lbResult.FontWeight = FontWeights.Bold;
                                                    lbResult.FontSize = 25;
                                                    lbResult.Foreground = Brushes.Red;
                                                }
                                                //ako pingujemo kasu
                                                else
                                                {
                                                    #region "Ovo treba ubaciti u ping kase"

                                                    //Provera da li su odhodi ok
                                                    if (CheckOdhodi())
                                                    {
                                                        lbResult.Content = $"Odhodi su OK :)";
                                                        lbResult.FontWeight = FontWeights.Normal;
                                                        lbResult.FontSize = 20;
                                                        BrushConverter bc = new BrushConverter();
                                                        lbResult.Foreground = (Brush)bc.ConvertFrom("#F4B41A");


                                                        //prikazi podatke blagajni
                                                        GetDataFromDB();

                                                        //Ako blagajna nije vec zatvorena
                                                        if (ProveraOdprtaOtpis(strDatum))
                                                        {

                                                            //Pokazi dugme zatvori
                                                            btnZatvori.Visibility = Visibility.Visible;

                                                        }
                                                        else
                                                        {
                                                            MessageBox.Show("Blagajna je vec zatvorena");
                                                        }



                                                    }
                                                    //Ako odhodi imaju vise od 2 file-a
                                                    else
                                                    {
                                                        lbResult.Content = "Odhodi nisu prazni, resetuj sepis";
                                                        lbResult.FontWeight = FontWeights.Bold;
                                                        lbResult.FontSize = 25;
                                                        lbResult.Foreground = Brushes.Red;

                                                    }

                                                    #endregion
                                                }
                                            }



                                        }//else od provere blagajne i poslovnice

                                    }//else od provere poslovnice

                                }
                                else
                                {
                                    MessageBox.Show("Ne pingujem server 192.168.1.46", "INFO!", MessageBoxButton.OK);
                                }



                            }//sve je ok

                        }

                    }

                }
            }



        }//Dugme Provera




        //Dugme ZATVORI !
        private void Zatvori_Click(object sender, RoutedEventArgs e)
        {
            //Provera da korisnik jos jednom potvrdi da zeli da zatvori blagajnu
            MessageBoxResult result = MessageBox.Show("Da li ste sigurni da zelite da zatvorite blagajnu?", "PROVERA", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);

            //No, cancel i izlaz svi treba da prekinu proces.
            //Samo Yes zatvara
            switch (result)
            {
                case MessageBoxResult.Yes:
                    #region "ovo ubaciti pod Da li ste sigurni"

                    //Ovo koristimo za proveru izabranog datuma
                    DateTime now = DateTime.Now;

                    //provera da li je dobro uneta poslovnica
                    if (!CheckPoslInput(txtPoslovnica.Text))
                    {
                        MessageBox.Show("Pogresno unet broj poslovnice");
                        txtPoslovnica.Focus();
                    }
                    //Provera da li je datum izabran
                    if (dtDatum.SelectedDate == null || dtDatum.SelectedDate >= DateTime.Today)
                    {
                        MessageBox.Show("Morate da izaberete datum i da bude pre danasnjeg");
                        dtDatum.Focus();
                    }
                    //Izabrani datum ne sme da bude stariji od 3 meseca
                    if (dtDatum.SelectedDate < now.AddDays(-8))
                    {
                        MessageBox.Show("Izabrani datum ne sme da bude stariji od 7 dana");
                        dtDatum.Focus();
                    }
                    //da li je dobro uneta blagajna
                    if (!CheckBlagInput(txtBlag.Text))
                    {
                        MessageBox.Show("Blagajna nije uneta kako treba");
                        txtBlag.Focus();
                    }
                    //Ako je sve ok
                    if (CheckPoslInput(txtPoslovnica.Text) && dtDatum.SelectedDate != null && dtDatum.SelectedDate < DateTime.Today && dtDatum.SelectedDate > now.AddDays(-8) && CheckBlagInput(txtBlag.Text))
                    {
                        if (!ProveraPosl())
                        {
                            MessageBox.Show("Poslovnica ne postoji u bazi");
                            txtPoslovnica.Focus();
                        }
                        //Poslovnica postoji u bazi
                        else
                        {

                            //pre pokretanja upita ka serveru pingujemo ga
                            if (PingServer("192.168.1.46"))
                            {
                                #region "pinguj server pre"
                                if (!ProveraBlag())
                                {
                                    MessageBox.Show("Blagajna ne postoji u bazi ili u toj poslovnici");
                                    txtBlag.Focus();
                                }
                                //ako je i blagajna i poslovnica u bazi
                                else
                                {
                                    DateTime datum = (DateTime)dtDatum.SelectedDate;
                                    //ovo je konverzija da vidim kako to treba da izgleda za prebacivanje u bazu
                                    string strDatum = datum.ToString("yyyyMMdd");

                                    string? ipKase = GetBlagIP();

                                    if (ipKase == null)
                                    {
                                        MessageBox.Show("IP BLAG JE NULL", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                    else
                                    {
                                        //ako ne pingujemo kasu
                                        if (!PingServer(ipKase))
                                        {
                                            MessageBox.Show("Ne pingujem kasu!", "INFO", MessageBoxButton.OK, MessageBoxImage.Warning);
                                            lbResult.Content = "Ne pingujem kasu!";
                                            lbResult.FontWeight = FontWeights.Bold;
                                            lbResult.FontSize = 25;
                                            lbResult.Foreground = Brushes.Red;
                                        }
                                        //ako pingujemo kasu
                                        else
                                        {
                                            #region "pinguj kasu"

                                            //Provera da li su odhodi ok
                                            if (CheckOdhodi())
                                            {
                                                lbResult.Content = $"Odhodi su OK :)";
                                                lbResult.FontWeight = FontWeights.Normal;
                                                lbResult.FontSize = 20;
                                                BrushConverter bc = new BrushConverter();
                                                lbResult.Foreground = (Brush)bc.ConvertFrom("#F4B41A");

                                                if (PingServer("192.168.1.46"))
                                                {
                                                    //prikazi podatke blagajni
                                                    GetDataFromDB();

                                                    //Ako blagajna nije vec zatvorena
                                                    if (ProveraOdprtaOtpis(strDatum))
                                                    {
                                                        //ZATVARA
                                                        UpdateZatvoriBlag(strDatum);

                                                        if (!ProveraOdprtaOtpis(strDatum))
                                                            MessageBox.Show($"Zatvorena je balgajna {txtBlag.Text} u poslovnici {txtPoslovnica.Text} za datum {datum:dd.MM.yyyy}");

                                                        GetDataFromDB();

                                                        lbResult.Content = $"Blagajna je zatvorena";
                                                        lbResult.FontWeight = FontWeights.Normal;
                                                        lbResult.FontSize = 20;
                                                        lbResult.Foreground = (Brush)bc.ConvertFrom("#F4B41A");

                                                        btnZatvori.Visibility = Visibility.Hidden;


                                                    }
                                                    else
                                                    {
                                                        MessageBox.Show("Blagajna je vec zatvorena");
                                                    }




                                                }
                                                else
                                                {
                                                    MessageBox.Show("Server is unaccessible", "INFO!", MessageBoxButton.OK);
                                                }



                                            }
                                            //Ako odhodi imaju vise od 2 file-a
                                            else
                                            {
                                                lbResult.Content = "Odhodi nisu prazni, resetuj sepis";
                                                lbResult.FontWeight = FontWeights.Bold;
                                                lbResult.FontSize = 25;
                                                lbResult.Foreground = Brushes.Red;

                                            }

                                            #endregion
                                        }
                                    }

                                }//else od provere blagajne i poslovnice

                                #endregion
                            }
                            else
                            {
                                MessageBox.Show("Ne pingujem server 192.168.1.46", "INFO!", MessageBoxButton.OK);
                            }

                        }//else od provere poslovnice


                    }//sve je ok


                    //da kada se jednom pritisne dugme zatvori, da nestane jer ne treba da se pritiska vise puta bez provere
                    btnZatvori.Visibility = Visibility.Hidden;
                    #endregion
                    break;
                case MessageBoxResult.No:
                    MessageBox.Show("Prekinut proces zatvaranja blagajne");
                    btnZatvori.Visibility = Visibility.Hidden;
                    break;
                case MessageBoxResult.Cancel:
                    MessageBox.Show("Prekinut proces zatvaranja blagajne");
                    btnZatvori.Visibility = Visibility.Hidden;
                    break;
                default:
                    MessageBox.Show("Prekinut proces zatvaranja blagajne");
                    btnZatvori.Visibility = Visibility.Hidden;
                    break;
            }



        }//Dugme ZATVORI


        #region "REGEX za posl i blag input"
        private bool CheckPoslInput(string s)
        {
            Regex regex = new Regex(@"2\d\d\d00");
            return regex.IsMatch(s);

        }

        //Metoda koja proverava input blagajne
        private bool CheckBlagInput(string s)
        {
            //proverimo prvo da li je poslovnica uneta kako treba
            if (CheckPoslInput(txtPoslovnica.Text))
            {
                //uzimamo 3 identifikaciona broja poslovnice
                string idPosl = txtPoslovnica.Text.Substring(1, 3);
                //blagajna mora da bude: prvi broj [1-9] pa onda 3 broja
                Regex regex = new Regex(@"[1-9]\d\d\d");
                //da li je uneta blagajna
                if (s == String.Empty)
                    return false;
                else
                {
                    //da li se poslednja 3 broja blagajne slazu sa identifikacionim brojem poslovnice
                    bool isIdOk = idPosl == s.Substring(1, 3);
                    //da li se ispostovala forma blagajne i slazu poslednja 3 broja sa identifikacionim brojem poslovnice
                    return regex.IsMatch(s) && isIdOk;
                }

            }
            else
            {
                MessageBox.Show("Poslovnica nije dobro uneta");
                return false;
            }

        }
        #endregion




        //Metoda koja proverava da li postoji uneta poslovnica u bazi.
        //Ako postoji vraca true ako ne postoji vraca false
        private bool ProveraPosl()
        {
            //ako je null ne postoji posl
            string? posl = null;

            try
            {

                string query = $"select POSL FROM BI_POSL WHERE POSL = {txtPoslovnica.Text} and AKTIVNA = 'D'";

                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    posl = reader.GetValue(0).ToString();

                }
                else
                {
                    posl = null;

                }

                connection.Close();
            }
            catch (Exception e)
            {
                posl = null;
                MessageBox.Show(e.ToString());
            }

            if (posl != null)
                return true;
            else
                return false;

        }


        //Metoda koja proverava da li postoji uneta blagajna u bazi.
        //Ako postoji vraca true ako ne postoji vraca false
        private bool ProveraBlag()
        {
            //ako je null ne postoji blag
            string? blag = null;

            try
            {
                string query = $"select BLAG_POSL from BI_BLAGPOSL where BLAG_POSL = {txtBlag.Text} and MATICNA_POSL = {txtPoslovnica.Text} and AKTIVNA = 'D'";

                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    blag = reader.GetValue(0).ToString();

                }
                else
                {
                    blag = null;

                }

                connection.Close();
            }
            catch (Exception e)
            {
                blag = null;
                MessageBox.Show(e.ToString());
            }

            if (blag != null)
                return true;
            else
                return false;

        }



        //Provera odhoda za test
        private int FilesInOdhodi(string path)
        {
            int fileCount = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly).Length;

            return fileCount;
        }



        //Provera da li su zatvorene blagajne
        //ako blagajna nije zatvorena vraca true
        //ako je blagajna zatvorena vraca false
        public bool ProveraOdprtaOtpis(string datum)
        {
            try
            {
                string query = $"select ODPRTA, TIP_ODPIS, BLAG_POSL from Bip_prom where KODA in ('BK','BZ') and POSL = {txtPoslovnica.Text} and DATUM = '{datum}' order by BLAG_POSL DESC";
                sqlConnection = new SqlConnection(connectionString);

                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query, sqlConnection);

                using (sqlDataAdapter)
                {
                    DataTable blagTable = new DataTable();
                    sqlDataAdapter.Fill(blagTable);

                    //prolaz kroz tabelu
                    for (int i = 0; i < blagTable.Rows.Count; i++)
                    {
                        //samo da gleda redove koji su za unetu blagajnu
                        if (blagTable.Rows[i]["BLAG_POSL"].ToString() == txtBlag.Text)
                        {
                            //Ako odprta nije * (tj null je)
                            if (blagTable.Rows[i]["ODPRTA"].ToString() != "*")
                            {
                                //ako tip odpisa nije 4
                                if (blagTable.Rows[i]["TIP_ODPIS"].ToString() != "4")
                                {
                                    return true;
                                }

                            }

                        }

                    }
                    //U suprotnom je blagajna zatvorena
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }


        //Pokretanje upita da prikaze podatke
        private void GetDataFromDB()
        {
            DateTime datum = (DateTime)dtDatum.SelectedDate;
            //ovo je konverzija da vidim kako to treba da izgleda za prebacivanje u bazu
            string strDatum = datum.ToString("yyyyMMdd");


            try
            {

                string query = $"select ODPRTA, TIP_ODPIS, BLAG_POSL, KODA from Bip_prom where KODA in ('BK','BZ') and POSL = {txtPoslovnica.Text} and DATUM = '{strDatum}' order by BLAG_POSL DESC";
                sqlConnection = new SqlConnection(connectionString);

                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query, sqlConnection);

                using (sqlDataAdapter)
                {
                    DataTable blagTable = new DataTable();
                    sqlDataAdapter.Fill(blagTable);



                    dataBaza.ItemsSource = blagTable.DefaultView;
                }
            }
            catch (Exception e)
            {

                MessageBox.Show(e.ToString());
            }


        }




        //dobijanje ip adrese blagajne
        //Ako iskoci NO DATA FOUND to znaci da nije nadjena ip adresa
        private string? GetBlagIP()
        {
            string? ipAdress = null;
            try
            {

                string query = $"select IP_LOKALNI_NASLOV, blag_posl from BI_BLAGPOSL where MATICNA_POSL = {txtPoslovnica.Text} and blag_posl = {txtBlag.Text}";
                sqlConnection = new SqlConnection(connectionString);

                sqlConnection.Open();

                SqlCommand cmd = new SqlCommand(query, sqlConnection);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    ipAdress = reader.GetValue(0).ToString();
                }
                else
                {
                    MessageBox.Show("NO DATA FOUND");
                }

            }
            catch (Exception e)
            {

                MessageBox.Show(e.ToString());

            }

            return ipAdress;
        }



        //Proverava odhode u kasi
        //ako ima manje od 3 file-a vraca true ako >= 3 vraca false
        private bool CheckOdhodi()
        {
            string? ipAdress = GetBlagIP();

            if (ipAdress != null)
            {
                string path = @"\\" + ipAdress + @"\RCL\xmlblag\odhodi";

                int numOfFiles = FilesInOdhodi(path);

                if (numOfFiles >= 3)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            //ako GetBlagIP() vrati null vrednost
            else
            {
                MessageBox.Show("IP BLAG JE NULL", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }


        }




        //Gets an ip adress in string form ("192.162.1.46") 
        //returns true or false based on weather it can see it
        private bool PingServer(string s)
        {
            //Za proveru da li imamo konekciju ka serveru
            Ping x = new Ping();
            PingReply reply = x.Send(IPAddress.Parse(s));

            Thread.Sleep(500);

            if (reply.Status == IPStatus.Success)
                return true;
            else
                return false;
        }




        //Metoda za zatvaranje blagajni
        private void UpdateZatvoriBlag(string datum)
        {
            try
            {

                string updateQuery = $"update BIP_PROM set ODPRTA = '*', TIP_ODPIS = '4' where KODA in ('BK','BZ') and POSL = {txtPoslovnica.Text} and DATUM = '{datum}' and BLAG_POSL = {txtBlag.Text}";

                sqlConnection = new SqlConnection(connectionString);

                sqlConnection.Open();

                SqlCommand cmd = new SqlCommand(updateQuery, sqlConnection);

                cmd.CommandText = updateQuery;

                //javlja koliko redova u bazi je izmenjeno
                MessageBox.Show($"Promenjeno je {cmd.ExecuteNonQuery()} redova");


                sqlConnection.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}
