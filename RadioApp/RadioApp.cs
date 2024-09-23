using System;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Policy;
using System.Windows.Forms;
using NAudio.Wave;

namespace RadioApp
{
    public partial class Form1 : Form
    {
        private IWavePlayer waveOut;
        private MediaFoundationReader mediaReader;
        private string currentStationUrl = "";
        private System.Windows.Forms.Timer progressTimer;
        private TimeSpan listeningTime;
        private Label programLabel;
        // Crear los botones de las radios
        string[] radioNames = { "LOS40", "LOS40 CLASSIC", "LOS40 DANCE", "LOS40 URBAN", "Alfa 91.3" };
        Color[] radioColors = { Color.Red, Color.Cyan, Color.Green, Color.Orange, Color.Pink };
        string[] radioUrls = {
                "http://playerservices.streamtheworld.com/api/livestream-redirect/LOS40_SC",
                "https://stream.zeno.fm/389cha4s3ehvv",
                "https://stream.zeno.fm/5ghqgm3h838uv",
                "https://playerservices.streamtheworld.com/api/livestream-redirect/LOS40_URBAN.mp3",
                "https://playerservices.streamtheworld.com/api/livestream-redirect/XHFAJ_FMAAC.aac"
        };
        string radioPhotosUrl = "https://sergiorex.es/radio/fotos/";
        PictureBox programImage;
        Button playButton;

        public Form1()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            // Configurar el color de fondo del formulario
            this.BackColor = Color.FromArgb(30, 30, 30); // Color gris oscuro
            this.ClientSize = new Size(800, 400);

            // Agregar el panel superior para las estaciones de radio
            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.Black
            };
            this.Controls.Add(topPanel);

            currentStationUrl = radioUrls[0];

            for (int i = 0; i < radioNames.Length; i++)
            {
                Button radioButton = new Button
                {
                    Text = radioNames[i],
                    ForeColor = Color.White,
                    BackColor = radioColors[i],
                    FlatStyle = FlatStyle.Flat,
                    Width = 150,
                    Height = 40,
                    Location = new Point(i * 160 + 10, 5),
                    Tag = radioUrls[i] // Asignar la URL de la radio como "Tag"
                };
                radioButton.FlatAppearance.BorderSize = 0;
                radioButton.Click += RadioButton_Click; // Manejar el evento Click
                topPanel.Controls.Add(radioButton);
            }

            //Cambiamos la foto
            ImageStream imageStream = new ImageStream();
            Image? image = imageStream.LoadImageFromUrl(radioPhotosUrl + "selecciona.png");

            // Panel central para la imagen del programa
            programImage = new PictureBox
            {
                Image = image, // Cambia esto por la ruta de tu imagen
                SizeMode = PictureBoxSizeMode.StretchImage,
                Location = new Point(20, 70),
                Size = new Size(150, 150)
            };
            this.Controls.Add(programImage);

            // Label para el nombre del programa
            programLabel = new Label
            {
                Text = "Selecciona una emisora",
                ForeColor = Color.White,
                Font = new Font("Arial", 14, FontStyle.Bold),
                Location = new Point(200, 100),
                AutoSize = true
            };
            this.Controls.Add(programLabel);

            // Botón de reproducción
            playButton = new Button
            {
                Text = "Play",
                Font = new Font("Arial", 12),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(255, 80, 80),
                Location = new Point(200, 150),
                Width = 100,
                Height = 50
            };
            playButton.Click += PlayButton_Click;
            this.Controls.Add(playButton);

            // Barra de progreso de la reproducción
            TrackBar progressBar = new TrackBar
            {
                Location = new Point(200, 220),
                Width = 400,
                Height = 20,
                Maximum = 100,
                Value = 0 // Inicializar en 0
            };
            this.Controls.Add(progressBar);

            // Trackbar para el control de volumen
            TrackBar volumeBar = new TrackBar
            {
                Location = new Point(620, 150),
                Width = 100,
                Height = 50,
                Minimum = 0,
                Maximum = 100,
                Value = 20 // Volumen inicial al 20%
            };
            volumeBar.Scroll += (sender, e) => ChangeVolume(volumeBar.Value);
            this.Controls.Add(volumeBar);

            // Label para mostrar el rango de horas
            Label timeRangeLabel = new Label
            {
                Text = "Volumen",
                ForeColor = Color.Gray,
                Font = new Font("Arial", 10),
                Location = new Point(635, 115),
                AutoSize = true
            };
            this.Controls.Add(timeRangeLabel);

            // Inicializar temporizador de progreso
            progressTimer = new System.Windows.Forms.Timer();
            progressTimer.Interval = 1000; // 1 segundo
            //progressTimer.Tick += Timer_Tick;
            

            // Label para mostrar el rango de horas
            Label timeLabel = new Label
            {
                Text = "Tiempo de escucha: 00:00:00",
                ForeColor = Color.Gray,
                Font = new Font("Arial", 10),
                Location = new Point(10, 370),
                AutoSize = true
            };
            this.Controls.Add(timeLabel);

            progressTimer.Tick += (s, ev) => Timer_Tick(timeLabel);

        }

        // Evento para cambiar la estación de radio
        private void RadioButton_Click(object sender, EventArgs e)
        {
            if (!true)
            {
                MessageBox.Show("Funcion Desactivada", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (sender is Button radioButton)
            {
                currentStationUrl = radioButton.Tag.ToString();
                StopRadio(); // Detener cualquier radio en reproducción
                PlayRadio(currentStationUrl); // Iniciar la nueva estación

                playButton.Text = "Pause";
            }
        }

        // Manejar la reproducción de la radio
        private void PlayRadio(string url)
        {
            try
            {
                waveOut = new WaveOutEvent();
                mediaReader = new MediaFoundationReader(url);
                waveOut.Init(mediaReader);
                waveOut.Play();

                // Iniciar el temporizador para mostrar el tiempo de escucha
                listeningTime = TimeSpan.Zero;
                progressTimer.Start();

                //Cambia el texto del titulo de la radio
                int index = Array.IndexOf(radioUrls, currentStationUrl);
                programLabel.Text = radioNames[index];

                //Cambiamos la foto
                ImageStream imageStream = new ImageStream();
                Image? image = imageStream.LoadImageFromUrl(radioPhotosUrl + index.ToString() + ".png");

                //use image
                programImage.Image = image;
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al reproducir la radio: " + ex.Message);
            }
        }

        // Evento del botón Play/Pause
        private void PlayButton_Click(object sender, EventArgs e)
        {

            if (waveOut == null && !string.IsNullOrEmpty(currentStationUrl))
            {
                PlayRadio(currentStationUrl);
                ((Button)sender).Text = "Pause";


                int index = Array.IndexOf(radioUrls, currentStationUrl);
                programLabel.Text = radioNames[index]; 

                ChangeVolume(20);
            }
            else if (waveOut != null)
            {
                if (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    waveOut.Pause();
                    ((Button)sender).Text = "Play";
                    progressTimer.Stop();

                    programLabel.Text = "Selecciona una emisora";
                }
                else
                {
                    waveOut.Play();
                    ((Button)sender).Text = "Pause";
                    progressTimer.Start();

                    int index = Array.IndexOf(radioUrls, currentStationUrl);
                    programLabel.Text = radioNames[index];
                }
            }
        }

        // Actualizar el tiempo de escucha
        private void Timer_Tick(Label time)
        {
            listeningTime = listeningTime.Add(TimeSpan.FromSeconds(1));
            UpdateTimeLabel(time);

        }

        // Actualiza el Label con el tiempo actual
        private void UpdateTimeLabel(Label time)
        {
            time.Text = "Tiempo de escucha: " + listeningTime.ToString(@"hh\:mm\:ss");
        }

        // Cambiar el volumen
        private void ChangeVolume(int volume)
        {
            if (waveOut != null)
            {
                waveOut.Volume = volume / 100f;
            }
        }

        // Detener la radio
        private void StopRadio()
        {
            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }

            if (mediaReader != null)
            {
                mediaReader.Dispose();
                mediaReader = null;
            }

            progressTimer.Stop(); // Detener el temporizador cuando se detiene la radio
        }
    }
}
