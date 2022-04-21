using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Net.Sockets;
using System.Net;
using System.Windows.Threading;

namespace Chat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Socket socket = null;

        DispatcherTimer dTimer = null; //creiamo il nostro dispatcher timer

        int PortaAscolto = 0;
        public MainWindow()
        {
            InitializeComponent();

            PortaAscolto = 65000;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); //addressFamily.interNetwork ci serve per specificare che la comunicazione avverrà attraverso l'IPv4,  SocketType.Dgram viene utilizzato per effettuare una connessione tramite l'udp, ProtocolType ci serve per specificare che utilizzeremo il protocollo UTP

            IPAddress local_address = IPAddress.Any; //indirizzo ip del mittente
            IPEndPoint local_endpoint = new IPEndPoint(local_address.MapToIPv4(), PortaAscolto); //indirizzo ip del destinatario. localAddress.MapToIPv4 serve per mappare a ipv4, ed il numero dopo la virgola è la porta. scegliamo la 65000 per evitare il blocco del firewall (dato che i firewall non dovrebbero bloccare le comunicazioni con porte >65000)

            socket.Bind(local_endpoint); //bind associa un socket all'end point. una volta fatto il bind posso stare in ascolto su quel socket per ricevere e trasmettere


            //parte per ascoltare i messaggi che ricevo

            dTimer = new DispatcherTimer(); //creo l'oggetto

            dTimer.Tick += new EventHandler(aggiornamento_dTimer); //aggiungo l'evento da eseguire ogni volta che scatta l'evento del timer (quindi ad ogni stop del timer io svolgo l'evento). Ciò che abbiamo tra parentesi è il nome del metodo che richiamiamo ad ogni stop del timer
            dTimer.Interval = new TimeSpan(0, 0, 0, 0, 250); //intervallo di tempo che stabiliamo tra un evento e quello successivo. in questo caso sono 250 millisecondi.
            dTimer.Start(); //avvio il timer



            lblPortaAscolto.Content = PortaAscolto;

        }

        private void btnInvia_Click(object sender, RoutedEventArgs e) //dobbiamo creare l'endpoint del destinatario, e mandargli i mex
        {
            IPAddress remote_address = IPAddress.Parse(txtIP.Text); //prendo l'ip che ho inserito nel textbox, e lo converto in un ip che sarà quello del destinatario (NON SONO SICURO CHE SIA IL DESTINATARIO)

            IPEndPoint remote_endpoint = new IPEndPoint(remote_address.MapToIPv4(), int.Parse(txtPorta.Text)); //è l'endpoint del destinatario

            //a questo punto devo inviare il messaggio
            byte[] messaggio = Encoding.UTF8.GetBytes(txtMessaggio.Text); //creo un vettore di byte e lo codifico in UTF8 partendo dal mex che ho sull'interfaccia

            socket.SendTo(messaggio, remote_endpoint); //inviamo il messaggio al destinatario
        }

        //un thread ascolterebbe in maniera costante, mentre noi in questa versione faremo si che tramite il dispatcher timer ci permetterà di ascoltare ad intervalli di tempo predefiniti
        
        private void aggiornamento_dTimer(object sender, EventArgs e) //metodo richiamato da ogni tick del timer, che avviene ogni volta che passa il tempo stabilito nel timeSpan
        {
            try
            {
                int nBytes = 0; //conto i bytes ricevuti, e mi serve per capire se almeno ho 1 byte da leggere, senno non faccio nulla (ottimizzazione)

                if ((nBytes = socket.Available) > 0) //controllo se il messaggio è vuoto o no
                {
                    //ricezione dei caratteri in attesa
                    byte[] buffer = new byte[nBytes]; //creo il vettore di bytes

                    //definisco il remote end point
                    EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0); //gli do dei valori di default

                    nBytes = socket.ReceiveFrom(buffer, ref remoteEndPoint); //ci permette di recuperare sia il messaggio, ma pure l'ip (dal pacchetto che è arrivato). quindi gli passo il buffer da cui arriva il messaggio, ed il riferimento all'ip di

                    string from = ((IPEndPoint)remoteEndPoint).Address.ToString(); //recupero l'indirizzo ip, e lo trasformo in stringa.
                    string messaggio = Encoding.UTF8.GetString(buffer, 0, nBytes);//faccio l'encoding in utf8 del messaggio che ho ricevuto. l'indice è da dove inizi a salvare, mettendo a 0 ovviamente parto dall'inizio

                    //trascrizione su listbox del from 
                    lstMessaggi.Items.Add(from + ": " + messaggio);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
