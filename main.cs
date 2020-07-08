using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;

namespace APIDemo
{
    public partial class main : Form
    {
        // ACA DEFINIMOS A EMGU.CV PARA LEVANTAR IMAGEN DE LA WEB CAM
        Image<Bgr, Byte> currentFrame;
        Capture grabber;

        // LA VARIABLE READY LO QUE HACE ES EVITAR QUE SE PIDA UNA NUEVA SOLICITUD SI UNA PREVIA NO TERMINO Y EVITAR ASI SATURACION Y LAGS
        Boolean Ready = true;

        // --------------------------------------------------------------------------------------------------------------------------------------------
        // ACA VAN LOS DATOS DEL SERVIDOR, LOS MISMO SE PUEDEN OBTENER DE LA APLICACION DE ADMINISTRACION O DESDE LA WEB DE ADMINISTRACION DEL SERVICIO


        String ServerAPIURL = "https://server1.proyectohorus.com.ar"; // << modificar por la URL del servidor en formato http://.........../produccion
        String ServerUser = ""; // << modificar por el usuario creado en el administrador de la API
        String ServerPassword = ""; // << modificar por la clave creada en el adminisrador de la API
        String Profile = ""; // << escribir el UUID correspondiente al perfil creado en el administrador de la API, el mismo debe estar bajo el miosmo usuario arriba escrito

        // --------------------------------------------------------------------------------------------------------------------------------------------


        // A LA VARIABLE TOKEN VA A IR A VARIAI ESA MEDIA LLAVE QUE SE REQUIERE PARA INTERACTUAR CON EL SERVICIO
        // EL TOKEN TIENE UNA DURACION DE 48HS, LUEGO SE DEBERA SOLICITAR UN NUEVO TOKE.
        String token = "";

        public main()
        {
            InitializeComponent();
        }

        public byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            // EN ESTA FUNCION LO QUE HACEMOS ES CONVERTIR UNA IMAGEN A JPG Y LUEGO A BYTE ARRAY
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }

        public async void GetToken()
        {
            // EN ESTA FUNCION OBTENGO EL TOKEN QUE ME PERMITIRA INTERACTUAR CON EL SERVICIO, ALGO ASI COMO EL USUARIO Y LA CONTRASEÑA
            if (token == "")
            {
                Ready = false;

                // DECLARAMOS LAS FUNCIONES NECESARIAS PARA ACCEDER AL SERVICIO ON LINE
                HttpClient httpClient = new HttpClient();
                MultipartFormDataContent form = new MultipartFormDataContent();
                HttpResponseMessage response;

                // DEFINIMOS LAS VARIABLES DE LA FUNCION
                form.Add(new StringContent(ServerUser), "user"); // user: es la primera de tres variables que toma "gettoken"
                form.Add(new StringContent(ServerPassword), "password"); // password es la segunda de tres variables que toma "gettoken"
                form.Add(new StringContent(Profile), "profileuuid"); // profileuuid: es la tercera de tres variables que toma "gettoken"


                // ENVIO LOS DATOS AL SERVIDOR
                response = await httpClient.PostAsync(ServerAPIURL + "/api/v2/functions/login", form);

                response.EnsureSuccessStatusCode();
                httpClient.Dispose();


                // COMO DEFINIMOS A outformat COMO pipe LA CADENA DE RETORNO SERA UN STRING SEPARADO POR "|", POR LO CUAL PARA OBTENER A CADA VALOR POR SEPARADO USAREMOS SPLIT('|'). 
                String[] RecivedMatrix = response.Content.ReadAsStringAsync().Result.Split('|');

                // SI OBTENGO UNA RESPUESTA CON EL CODIGO 200 EN LA POSICION 0 DEL STREAM DE RESPUESTA SIGNIFICA QUE EN LA POSICION 1 SE ENTREGO EL TOKEN
                if (RecivedMatrix[0] == "200")
                {
                    token = RecivedMatrix[1];
                }
                else
                {
                    token = "";
                }

                Ready = true;
            }
        }

        private async void Upload()
        {
            // EN ESTA FUNCION HACEMOS EL UPLOAD DE LA IMAGEN A EVALUAR Y MUESTRA EL RESULTADO
            try
            {
                // LO PRIMERO QUE HACEMOS ES CONSULTAR A LA VARIABLE READY PARA VER SI AUN HAY UNA RESPUESTA POR OBTENER DEL SERVIDOR ANTES DE ENVIAR UNA NUEVA CONSULTA
                if (Ready == true)
                {
                    // LLAMAMOS A LA FUNCION DE TOKEN
                    GetToken();

                    // DECLARAMOS LAS FUNCIONES NECESARIAS PARA ACCEDER AL SERVICIO ON LINE
                    HttpClient httpClient = new HttpClient();
                    MultipartFormDataContent form = new MultipartFormDataContent();
                    HttpResponseMessage response;

                    // EN Recivedtmp VAMOS A GUARDAR LA RESPUESTA QUE LLEGUE DESDE EL SERVIDOR
                    String Recivedtmp = "";

                    // DEFINIMOS LAS VARIABLES DE LA FUNCION
                    httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token); // token: Excepto en la funcion "gettoken", en todas las demas deberemos enviar el token generado justamente por "gettoken" ya que es el usuario y la clave que nos permite acceder al servicio, sin esto recibiremos un mensaje de usuario incorrecto
                    form.Add(new ByteArrayContent(imageToByteArray(currentFrame.ToBitmap()), 0, imageToByteArray(currentFrame.ToBitmap()).Length), "photo", "image"); // photo: Envia el bytestream de la imagen la cual debe estar en formato JPG.

                    // ENVIO LOS DATOS AL SERVIDOR
                    response = await httpClient.PostAsync(ServerAPIURL + "/api/v2/functions/face/id", form);

                    response.EnsureSuccessStatusCode();
                    httpClient.Dispose();

                    // RECIBIMOS LA RESPUESTA DESDE EL SERVIDOR
                    Recivedtmp = response.Content.ReadAsStringAsync().Result;


                    // EL STRING NOS DEVOLVERA UN UUID COMO IDENTIFICACION DEL OBJETO O ROSTRO DETECTADO POR LO CUAL DEBEREMOS REMPLAZARLO POR LA INFORMACION CANONICA QUE DECLARAMOS EN EL ADMINISTRADOR DE LA API
                    if (Recivedtmp.Trim() != "")
                    {
                        // LO PRIMERO QUE HACEMOS ES SEPARAR LA RESPUESTA ES LINEAS YA QUE outformat LO DEFINIMOS COMO pipe, PUEDEN HABER MAS DE UNA RESPUESTA SI ES QUE EN LA IMAGEN HAY MAS DE UN OBJETO DETECTABLE (EJEMPLO: HAY DOS ROSTROS)
                        String[] Metadata = Recivedtmp.Split('\n');

                        // EVALUAMOS UNA A UNA LAS RESPUESTAS
                        foreach (String Metaline in Metadata)
                        {
                            // SI LA LINEA NO ESTA VACIA IMPLICA QUE HAY UNA RESPUESTA POR PARTE DEL SERVIDOR
                            if (Metaline.Trim() != "")
                            {
                                // SEPARAMOS LAS LINEAS EN "|" YA QUE outformat LO DEFINIMOS COMO pipe
                                String[] Values = Metaline.Split('|');


                                WebClient webClient = new WebClient();
                                webClient.Headers.Add("Authorization", "Bearer " + token);

                                String response1 = webClient.DownloadString(ServerAPIURL + "/api/v2/admin/accounts/users/profiles/detections=" + Values[6] + "/value");

                                String[] RecivedMatrix1 = response1.Split('|');

                                // SI EL CODIGO RECIBIDO EN LA POSICION O ES 200 SIGNIFICA QUE EL SERVIDOR RESPONDIO CORRECTAMENTE CON EL CANONICO DEL UUID POR LO CUAL PROCEDEMOS A REMPLAZARLO EN EL STRING DE DETECCION
                                if (RecivedMatrix1[0] == "200")
                                    Recivedtmp = Recivedtmp.Replace(Values[6], RecivedMatrix1[1]);

                                // YA CON TODO RECIBIDO Y FORMATEADO PASAMOS A INTERPRETAR LA INFORMACION.
                                String[] ReceiveOnMatrix = Recivedtmp.Split('|');

                                // COMO TENEMOS DEFINIDA LA RESPUESTA CON outformat EN pipe, CADA DETECCION SERA UN STREAM QUE OCUPARA UNA LINEA DONDE CADA RESPUESTA OCUPA UNA POSICION SEPARADA PO "|"

                                // EJEMPLO DEL STREAM EN FORMATO PIPE:

                                // [CODIGO DE ACCION]|[CANONICO DEL CODIGO]|[POSICION YMIN DEL BOX DE DETECCION]|[POSICION XMAX DEL BOX DE DETECCION]|[POSICION YMAX DEL BOX DE DETECCION]|[POSICION XMAX DEL BOX DE DETECCION]|[NOMBRE O UUID DE LO DETECTADO]|[UUID DEL GRUPO IRIS]|[CONFIDENCE DE LA DETECCION O SEA QUE TAN EXACTA ES LA DETECCION]
                                
                                // EJEMPLO PRACTICO: 
                                
                                //  200|ok|0.0|0.44375|0.47291666666666665|0.7140625|Juan Perez|1cc9a3d4461011ea9ca300155d016a1c|0.4329930749381952\n


                                if (ReceiveOnMatrix[0] == "200")
                                {
                                    // LOS VALORES X e Y ESTAN SIN ESCALAR ESTO SIGNIFICA QUE DEBEREMOS MULTIPLICAR CADA VALOR POR EL ANCHO Y EL ALTO DE LA IMAGEN PARA OBTENER LAS COORDENADAS X/Y SOBRE LA IMAGEN 

                                    Double ymin = Convert.ToDouble(ReceiveOnMatrix[2]);
                                    Double xmin = Convert.ToDouble(ReceiveOnMatrix[3]);
                                    Double ymax = Convert.ToDouble(ReceiveOnMatrix[4]);
                                    Double xmax = Convert.ToDouble(ReceiveOnMatrix[5]);

                                    Int32 left = Convert.ToInt32(xmin * 640);
                                    Int32 right = Convert.ToInt32(xmax * 640);
                                    Int32 top = Convert.ToInt32(ymin * 480);
                                    Int32 bottom = Convert.ToInt32(ymax * 480);

                                    // POR ULTIMO SEPRAMOS LOS DATOS Y LOS PRESENTAMOS
                                    String Name = ReceiveOnMatrix[6];
                                    Double Confidence = Convert.ToDouble(ReceiveOnMatrix[8]);

                                    this.FromServer.Text = "Top: " + top.ToString() + " | Left: " + left.ToString() + " | Bottom: " + bottom.ToString() + " | Right: " + right.ToString() + " | Name: " + Name + " | Confidence: " + Confidence.ToString();
                                }
                            }
                        }
                    }

                    Ready = true;
                }
            }
            catch (Exception ex)
            {
                Ready = true;
            }
        }

        void Setup()
        {
            // INICIAMOS LA CAPTURA VIA WEB CAM. PODEMOS CAMBIAR EL VALOR "0" DE Capture(0) POR LA DIRECCION DE UNA IMAGEN EN DISCO, UNA DIRECCION A UN STREAM DE VIDEO O VIDEO LOCAL. SI TENEMOS MAS DE UNA CAMARA EN LA PC PODREMOS CAMBIAR EL "0" POR OTRO NUMERO QUE DE ACCESO A LA CAMARA CORRECTRA
            grabber = new Capture(0);
            grabber.QueryFrame();
            Application.Idle += new EventHandler(Loop);
        }

        private void main_Load(object sender, EventArgs e)
        {
            Setup();
        }

        void Loop(object sender, EventArgs e)
        {
            // ESTE ES EL LOOP DONDE OBTENEMOS EL FRAME DE CAPTURA Y LO MANDAMOS A UPLOAD PARA SER IDENTIFICADO
            currentFrame = grabber.QueryFrame().Resize(640, 480, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            this.Preview.Image = currentFrame.ToBitmap();

            Upload();
        }
    }
}
