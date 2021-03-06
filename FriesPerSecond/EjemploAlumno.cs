using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Sound;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSkeletalAnimation;
using TgcViewer.Utils.Terrain;
using System.Windows.Forms;
using TgcViewer.Utils._2D;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.Shaders;

namespace AlumnoEjemplos.FriesPerSecond
{

    /// <summary>
    /// Ejemplo del alumno
    /// </summary>
    public class EjemploAlumno : TgcExample
    {
        Device d3dDevice;
        //Escena
        TgcScene scene;

        //Meshes
        TgcBox piso;
        TgcSimpleTerrain terreno;
        TgcSkyBox skyBox;
        TgcBoundingBox bbSkyBox;
        TgcSkeletalMesh originalEnemigo;
        List<Enemigo> instanciasEnemigos;
        TgcMesh palmeraOriginal;
        TgcMesh otroArbolOriginal;
        TgcMesh pastoOriginal;
        TgcMesh piedraOriginal;
        List<TgcMesh> pasto;
        List<TgcMesh> arboles;
        List<TgcMesh> piedras;
        List<TgcBoundingBox> BBZona1;
        List<TgcBoundingBox> BBZona2;
        List<TgcBoundingBox> BBZona3;
        List<TgcBoundingBox> BBZona4;
        TgcText2d vida;
        Vector2 posicionArmaDisparo;
        Vector2 posicionArmaOriginal;
        TgcBox personaje;
        TgcMesh barril;
        Barril barrilDisparado;
        TgcMesh esferaExplosion;
        List<Barril> barriles;
        List<TgcMesh> explosiones;
        TgcMesh logoTgc;
        TgcScene sceneLogo;



        Quadtree qt;
        List<TgcMesh> totales;

        Effect effect;
        float time;
        Random rand;

        //Sonidos
        TgcStaticSound disparo;
        TgcStaticSound headshot;
        TgcStaticSound golpe;
        TgcStaticSound explosion;
        TgcStaticSound muerte;
        TgcStaticSound ganador;
        TgcStaticSound sorpresa;

        protected Point mouseCenter;

        //Bounding
        TgcBoundingBox boundingCamara;
        Vector3 boundingCamScale;
        Vector3 ultimaPosCamara;
        TgcBoundingSphere boundingBarril;


        //Mira
        TgcSprite mira;
        TgcSprite mira_zoom;

        //Arma
        TgcSprite arma;
        TgcAnimatedSprite fuegoArma;
        int cantF = 0;

        //Disparo
        Bala unaBala;
        bool huboDisparo;
        bool disparoBarril;
        TgcBox puntoDisparo;
        Vector3 col;
        Vector3 posicionRayBala;

        //Banderas
        bool miraActivada;

        //Variables
        float anguloFov;
        float aspectRatio;
        float velocidadAngular = 1.75f;
        float velocidadMov = 750f;
        float velocidadCorrer = 1500f;
        float ruedita;
        float velocidadEnemigos = -5f;
        float velocidadBala = 1000f;
        float numVida = 100f;
        int puntaje;
        int puntajeRecord;
        TgcText2d textoPuntaje;
        TgcText2d textoPerdiste;
        TgcText2d textoGanaste;
        TgcText2d textoPuntajeFinal;
        TgcText2d textoPuntajeRecord;
        bool primeraVez;
        string mediaPath;
        TgcSkeletalLoader loader;
        string alumnoMediaFolder;
        int numExplosion;
        float[] lightPos = new float[] { 0, 1900, 300 };
        bool reproducirSorpresa;

        //Musica
        //TgcMp3Player musicaFondo = GuiController.Instance.Mp3Player;
        string pathMusica;
        TgcMp3Player player;

        //Camara
        Q3FpsCamera camaraQ3;

        //Estado juego
        enum estado { jugar, menu, instrucciones, creditos, muerto, ganador };
        estado estadoJuego;
        Size screenSize;
        

        //Menu
        TgcSprite fondoMenu;
        TgcSprite titulo;
        TgcSprite instrucciones;
        TgcSprite creditos;
        TgcSprite botonJugar;
        TgcSprite botonInstrucciones;
        TgcSprite botonCreditos;
        TgcSprite botonSalir;
        TgcSprite botonVolver;
        TgcSprite barraVida;
        Size sizeJugar;
        Size sizeInstrucciones;
        Size sizeCreditos;
        Size sizeSalir;
        Size sizeVolver;

        /// <summary>
        /// Categor�a a la que pertenece el ejemplo.
        /// Influye en donde se va a haber en el �rbol de la derecha de la pantalla.
        /// </summary>
        public override string getCategory()
        {
            return "AlumnoEjemplos";
        }

        /// <summary>
        /// Completar nombre del grupo en formato Grupo NN
        /// </summary>
        public override string getName()
        {
            return "FriesPerSecond";
        }

        /// <summary>
        /// Completar con la descripci�n del TP
        /// </summary>
        public override string getDescription()
        {
            return "MiIdea - Descripcion de la idea";
        }

        /// <summary>
        /// M�todo que se llama una sola vez,  al principio cuando se ejecuta el ejemplo.
        /// Escribir aqu� todo el c�digo de inicializaci�n: cargar modelos, texturas, modifiers, uservars, etc.
        /// Borrar todo lo que no haga falta
        /// </summary>
        public override void init()
        {
            //GuiController.Instance: acceso principal a todas las herramientas del Framework

            //Device de DirectX para crear primitivas
            d3dDevice = GuiController.Instance.D3dDevice;

            rand = new Random();
            
            //Carpeta de archivos Media del alumno
            alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosMediaDir;

            effect = TgcShaders.loadEffect(alumnoMediaFolder + "Shaders\\viento.fx");

            Control focusWindows = GuiController.Instance.D3dDevice.CreationParameters.FocusWindow;
            mouseCenter = focusWindows.PointToScreen(
                new Point(
                    focusWindows.Width / 2,
                    focusWindows.Height / 2)
                    );

            inicializarTerreno();

            pathMusica = GuiController.Instance.AlumnoEjemplosMediaDir + "Sonidos\\Stayin_alive.mp3";
            GuiController.Instance.Mp3Player.closeFile();
            GuiController.Instance.Mp3Player.FileName = pathMusica;

            player = GuiController.Instance.Mp3Player;

            //player.play(true);

            //Crear Sprite
            mira = new TgcSprite();
            mira.Texture = TgcTexture.createTexture(alumnoMediaFolder + "\\mira.png");
            miraActivada = false;
            mira_zoom = new TgcSprite();
            mira_zoom.Texture = TgcTexture.createTexture(alumnoMediaFolder + "\\mira_zoom3.png");

            arma = new TgcSprite();
            arma.Texture = TgcTexture.createTexture(alumnoMediaFolder + "\\arma.png");

            fuegoArma = new TgcAnimatedSprite(
                GuiController.Instance.ExamplesMediaDir + "\\Texturas\\Sprites\\Explosion.png", //Textura de 256x256
                new Size(64, 64), //Tama�o de un frame (64x64px en este caso)
                16, //Cantidad de frames, (son 16 de 64x64px)
                10 //Velocidad de animacion, en cuadros x segundo
                );

            //Sonidos
            disparo = new TgcStaticSound();
            disparo.loadSound(alumnoMediaFolder + "\\Sonidos\\disparo.wav");

            headshot = new TgcStaticSound();
            headshot.loadSound(alumnoMediaFolder + "\\Sonidos\\Headshot.wav");

            golpe = new TgcStaticSound();
            golpe.loadSound(alumnoMediaFolder + "\\Sonidos\\punch.wav");

            explosion = new TgcStaticSound();
            explosion.loadSound(alumnoMediaFolder + "\\Sonidos\\explosion.wav");

            muerte = new TgcStaticSound();
            muerte.loadSound(alumnoMediaFolder + "\\Sonidos\\gameover.wav");

            ganador = new TgcStaticSound();
            ganador.loadSound(alumnoMediaFolder + "\\Sonidos\\ganador.wav");
            
            sorpresa = new TgcStaticSound();
            sorpresa.loadSound(alumnoMediaFolder + "\\Sonidos\\sorpresa.wav");

            

            //Ubicarlo centrado en la pantalla
            screenSize = GuiController.Instance.Panel3d.Size;
            Size textureSize = mira.Texture.Size;
            mira.Scaling = new Vector2(0.6f, 0.6f);
            mira.Position = new Vector2(FastMath.Max(screenSize.Width / 2 - (textureSize.Width * 0.6f) / 2, 0), FastMath.Max(screenSize.Height / 2 - (textureSize.Height * 0.6f) / 2, 0));

            mira_zoom.Scaling = new Vector2((float)screenSize.Width / mira_zoom.Texture.Size.Width, (float)screenSize.Height / mira_zoom.Texture.Size.Height);
            mira_zoom.Position = new Vector2(0, 0);

            Size armaSize = arma.Texture.Size;
            float escalaAncho = (screenSize.Width / 2f) / armaSize.Width;

            arma.Scaling = new Vector2(escalaAncho, escalaAncho);
            arma.Position = new Vector2(screenSize.Width - (armaSize.Width * escalaAncho), screenSize.Height - (armaSize.Height * escalaAncho));

            posicionArmaDisparo = new Vector2(arma.Position.X + 5f, arma.Position.Y + 5f);
            posicionArmaOriginal = arma.Position;

            fuegoArma.Position = arma.Position + new Vector2(22f,28f);

            

            

            inicializarArboles();
            inicializarPasto();
            inicializarPiedras();

            totales = new List<TgcMesh>();
            totales.AddRange(arboles);
            totales.AddRange(pasto);
            totales.AddRange(piedras);

            qt = new Quadtree();
            qt.create(totales, bbSkyBox);
            qt.createDebugQuadtreeMeshes();

            //Crear texto vida, b�sico
            vida = new TgcText2d();
            vida.Text = "100";
            vida.Color = Color.White;
            vida.Size = new Size(300, 100);
            vida.changeFont(new System.Drawing.Font("BankGothic Md BT", 25, FontStyle.Bold));
            vida.Position = new Point(-60, 0);

            barraVida = new TgcSprite();
            barraVida.Texture = TgcTexture.createTexture(alumnoMediaFolder + "\\barra_vida.png");
            barraVida.Position = new Vector2((float)vida.Position.X + 175f, (float)vida.Position.Y);
            barraVida.Scaling = new Vector2(0.3f, 0.3f);

            textoPuntaje = new TgcText2d();
            textoPuntaje.Text = "Puntos: 0";
            textoPuntaje.Color = Color.White;
            textoPuntaje.Size = new Size(300, 100);
            textoPuntaje.changeFont(new System.Drawing.Font("BankGothic Md BT", 25, FontStyle.Bold));
            textoPuntaje.Position = new Point(screenSize.Width - 300, 0);

            TgcSceneLoader loaderLogo = new TgcSceneLoader();
            logoTgc = loaderLogo.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\LogoTGC\\LogoTGC-TgcScene.xml").Meshes[0];
            logoTgc.move(new Vector3(0f, 1900f, 0f));
            logoTgc.Scale = new Vector3(14f, 14f, 14f);
            
            //Cargar Shader de PhongShading
            logoTgc.Effect = GuiController.Instance.Shaders.TgcMeshPhongShader;
            logoTgc.Technique = GuiController.Instance.Shaders.getTgcMeshTechnique(logoTgc.RenderType);
            //Cargar variables shader
            logoTgc.Effect.SetValue("ambientColor", ColorValue.FromColor(Color.Gray));
            logoTgc.Effect.SetValue("diffuseColor", ColorValue.FromColor(Color.LightBlue));
            logoTgc.Effect.SetValue("specularColor", ColorValue.FromColor(Color.White));
            logoTgc.Effect.SetValue("specularExp", 20f);
            logoTgc.Effect.SetValue("lightPosition", lightPos);
            reproducirSorpresa = false;

            


            //////VARIABLES DE FRUSTUM

            aspectRatio = (float)GuiController.Instance.Panel3d.Width / GuiController.Instance.Panel3d.Height;

            ruedita = 0f;

            //Inicializo angulo de FOV
            anguloFov = FastMath.ToRad(45.0f);

            GuiController.Instance.D3dDevice.Transform.Projection = Matrix.PerspectiveFovLH(anguloFov, aspectRatio, 1f, 50000f);

            ///////////////USER VARS//////////////////
            /*
            //Crear una UserVar
            GuiController.Instance.UserVars.addVar("variablePrueba");

            //Cargar valor en UserVar
            GuiController.Instance.UserVars.setValue("variablePrueba", 5451);
            */

            ///////////////MODIFIERS//////////////////

           /*
            //Crear un modifier para un valor FLOAT
            GuiController.Instance.Modifiers.addFloat("valorFloat", -50f, 200f, 0f);

            //Crear un modifier para un ComboBox con opciones
            string[] opciones = new string[] { "opcion1", "opcion2", "opcion3" };
            GuiController.Instance.Modifiers.addInterval("valorIntervalo", opciones, 0);

            //Crear un modifier para modificar un v�rtice
            GuiController.Instance.Modifiers.addVertex3f("valorVertice", new Vector3(-100, -100, -100), new Vector3(50, 50, 50), new Vector3(0, 0, 0));
            */


            ///////////////CONFIGURAR CAMARA ROTACIONAL//////////////////
            //Es la camara que viene por default, asi que no hace falta hacerlo siempre
            //GuiController.Instance.RotCamera.Enable = true;
            //Configurar centro al que se mira y distancia desde la que se mira
            //GuiController.Instance.RotCamera.setCamera(new Vector3(0, 0, 0), 300);
            Vector3 posicion = new Vector3(0f, 150f, 0f);
            /*GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(0,120,0), new Vector3(1, 0, 1));
            //GuiController.Instance.FpsCamera.LookAt(new Vector3(0,120,0));
            GuiController.Instance.FpsCamera.JumpSpeed = 0;
            GuiController.Instance.FpsCamera.MovementSpeed *= 10;*/

            GuiController.Instance.FpsCamera.Enable = false;
            GuiController.Instance.RotCamera.Enable = false;

            camaraQ3 = new Q3FpsCamera();
            camaraQ3.setCamera(posicion, posicion + new Vector3(1.0f, 0.0f, 0.0f));
            camaraQ3.RotationSpeed = velocidadAngular;
            camaraQ3.MovementSpeed = velocidadMov;
            camaraQ3.LockCam = false;

            ultimaPosCamara = camaraQ3.getPosition();
            Vector3 posBound = new Vector3(camaraQ3.getPosition().X, camaraQ3.getPosition().Y + 30, camaraQ3.getPosition().Z);
            boundingCamara = new TgcBoundingBox();
            boundingCamScale = new Vector3(1f, 1f, 1f);
            boundingCamara.scaleTranslate(posBound, boundingCamScale);

            personaje = TgcBox.fromSize(new Vector3(30f, 60f, 30f), Color.Red);
            personaje.Position = camaraQ3.getPosition();
            personaje.move(new Vector3(0f, -30f, 0f));
            
           
            //ENEMIGOS          
            instanciasEnemigos = new List<Enemigo>();
            //El ultimo parametro es el radio
            inicializarEnemigos(4, 4, instanciasEnemigos, 3.4f, 200.0f);

            crearEsferaExplosion();
            inicializarBarriles();

            //Para disparo
            col = new Vector3(0f, 0f, 0f);
            huboDisparo = false;
            disparoBarril = false;
            unaBala = new Bala();
            puntoDisparo = TgcBox.fromSize(new Vector3(10f, 10f, 10f), Color.Red);

            #region menu
            //Defino el estado inicial como menu
            estadoJuego = estado.menu;

            //Sprites para menu
            fondoMenu = new TgcSprite();
            fondoMenu.Texture = TgcTexture.createTexture(alumnoMediaFolder + "\\fondo_menu.jpg");

            titulo = new TgcSprite();
            titulo.Texture = TgcTexture.createTexture(alumnoMediaFolder + "\\Menu\\titulo.png");
            titulo.Scaling = new Vector2(0.5f, 0.5f);
            titulo.Position = new Vector2((screenSize.Width / 2) - titulo.Texture.Width / 4, (titulo.Texture.Height / 2)-50f);      

            instrucciones = new TgcSprite();
            instrucciones.Texture = TgcTexture.createTexture(alumnoMediaFolder + "\\Menu\\instrucciones.png");
            instrucciones.Scaling = new Vector2(0.4f, 0.5f);
            instrucciones.Position = new Vector2((screenSize.Width / 2) - (instrucciones.Texture.Width*0.4f) / 2, (instrucciones.Texture.Height / 2) - instrucciones.Texture.Height / 2);

            creditos = new TgcSprite();
            creditos.Texture = TgcTexture.createTexture(alumnoMediaFolder + "\\Menu\\creditos.png");
            creditos.Scaling = new Vector2(0.5f, 0.5f);
            creditos.Position = new Vector2((screenSize.Width / 2) - creditos.Texture.Width / 4, (creditos.Texture.Height / 2) - creditos.Texture.Height / 2);

            botonJugar = new TgcSprite();
            botonJugar.Texture = TgcTexture.createTexture(alumnoMediaFolder + "\\Menu\\boton_jugar.png"); 
            botonJugar.Scaling = new Vector2(0.5f, 0.5f);
            sizeJugar = botonJugar.Texture.Size;
           // sizeJugar.Width = sizeJugar.Width / 2;
            botonJugar.Position = new Vector2((screenSize.Width / 2)-sizeJugar.Width/4, (screenSize.Height / 2)-sizeJugar.Height/4);


            botonInstrucciones = new TgcSprite();
            botonInstrucciones.Texture = TgcTexture.createTexture(alumnoMediaFolder + "\\Menu\\boton_instrucciones.png");
            botonInstrucciones.Scaling = new Vector2(0.5f, 0.5f);
            sizeInstrucciones = botonInstrucciones.Texture.Size;
            //sizeInstrucciones.Width = sizeInstrucciones.Width / 2;
            botonInstrucciones.Position = new Vector2((screenSize.Width / 2) - sizeInstrucciones.Width / 4, (screenSize.Height / 2) - sizeInstrucciones.Height / 4 + 85f);
            
                  
            botonCreditos = new TgcSprite();
            botonCreditos.Texture = TgcTexture.createTexture(alumnoMediaFolder + "\\Menu\\boton_creditos.png");
            botonCreditos.Scaling = new Vector2(0.5f, 0.5f);
            sizeCreditos = botonCreditos.Texture.Size;
            //sizeCreditos.Width = sizeCreditos.Width / 2;
            botonCreditos.Position = new Vector2((screenSize.Width / 2) - sizeCreditos.Width / 4, (screenSize.Height / 2) - sizeCreditos.Height / 4 + sizeInstrucciones.Height + 50f);

            textoPerdiste = new TgcText2d();
            textoPerdiste.Text = "Game over";
            textoPerdiste.Align = TgcText2d.TextAlign.CENTER;
            textoPerdiste.Color = Color.Black;
            textoPerdiste.Size = new Size(600, 100);
            textoPerdiste.Position = new Point((screenSize.Width / 2) - textoPerdiste.Size.Width / 2, (screenSize.Height / 2) - textoPerdiste.Size.Height / 2);
            textoPerdiste.changeFont(new System.Drawing.Font("BankGothic Md BT", 50, FontStyle.Bold));

            textoGanaste = new TgcText2d();
            textoGanaste.Text = "Ganaste!";
            textoGanaste.Align = TgcText2d.TextAlign.CENTER;
            textoGanaste.Color = Color.Black;
            textoGanaste.Size = new Size(600, 100);
            textoGanaste.Position = new Point((screenSize.Width / 2) - textoGanaste.Size.Width / 2, (screenSize.Height / 2) - textoGanaste.Size.Height / 2);
            textoGanaste.changeFont(new System.Drawing.Font("BankGothic Md BT", 50, FontStyle.Bold));

            textoPuntajeFinal = new TgcText2d();
            textoPuntajeFinal.Color = Color.Black;
            textoPuntajeFinal.changeFont(new System.Drawing.Font("BankGothic Md BT", 25, FontStyle.Bold));
            textoPuntajeFinal.Size = new Size(800, 100);
            textoPuntajeFinal.Position = new Point((screenSize.Width / 2) - textoPuntajeFinal.Size.Width / 2, (screenSize.Height / 2) - textoPuntajeFinal.Size.Height/2 + textoGanaste.Size.Height);

            textoPuntajeRecord = new TgcText2d();
            textoPuntajeRecord.Color = Color.Black;
            textoPuntajeRecord.Size = new Size(400, 60);
            textoPuntajeRecord.changeFont(new System.Drawing.Font("BankGothic Md BT", 15, FontStyle.Bold));
            textoPuntajeRecord.Position = new Point((screenSize.Width - textoPuntajeRecord.Size.Width),(screenSize.Height-textoPuntajeRecord.Size.Height/2));
            puntajeRecord = Int32.Parse(System.IO.File.ReadAllText(alumnoMediaFolder + "\\record.txt"));
            textoPuntajeRecord.Text = "Tu puntaje record es: " + puntajeRecord.ToString();

            primeraVez = true;
            #endregion menu



            

        }
        public void crearEsferaExplosion()
        {
            TgcSceneLoader loaderExplosion = new TgcSceneLoader();
            esferaExplosion = loaderExplosion.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "\\Sphere\\Sphere-TgcScene.xml").Meshes[0];
            esferaExplosion.changeDiffuseMaps(new TgcTexture[] { TgcTexture.createTexture(d3dDevice, GuiController.Instance.AlumnoEjemplosMediaDir + "\\explosion.jpg") });
            
        }

        public override void render(float elapsedTime)
        {
            //Device de DirectX para renderizar
            d3dDevice = GuiController.Instance.D3dDevice;
            time += elapsedTime;

            TgcD3dInput input = GuiController.Instance.D3dInput;
            
            switch (estadoJuego)
            {
                case estado.menu:
                    menu(input);
                    break;
                case estado.jugar:
                    
                    jugar(input, elapsedTime);
                    break;
                case estado.instrucciones:
                    verInstrucciones();
                    break;
                case estado.creditos:
                    verCreditos();
                    break;
                case estado.muerto:
                    cartelMuerte(input);
                    break;
                case estado.ganador:
                    cartelGanador(input);
                    break;
            }
        }

        private void cartelGanador(TgcD3dInput input)
        {
            GuiController.Instance.Drawer2D.beginDrawSprite();
            fondoMenu.render();

            if ( GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.M))
            {
                puntajeRecord = Int32.Parse(System.IO.File.ReadAllText(alumnoMediaFolder + "\\record.txt"));
                textoPuntajeRecord.Text = "Tu puntaje record es: " + puntajeRecord.ToString();
                estadoJuego = estado.menu;
            }
            GuiController.Instance.Drawer2D.endDrawSprite();
            textoPuntajeFinal.render();
            textoGanaste.render();
            player.stop();
        }

        private void cartelMuerte(TgcD3dInput input)
        {
            GuiController.Instance.Drawer2D.beginDrawSprite();
            fondoMenu.render();
           
            if ( GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.M))
            {
                estadoJuego = estado.menu;
            }
            GuiController.Instance.Drawer2D.endDrawSprite();
            textoPerdiste.render();
            player.stop();
        }

        private void verCreditos()
        {
            GuiController.Instance.Drawer2D.beginDrawSprite();
            fondoMenu.render();
            creditos.render();
            GuiController.Instance.Drawer2D.endDrawSprite();
            if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.Back) || GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.M))
            {
                estadoJuego = estado.menu;
            }
        }

        private void verInstrucciones()
        {
            camaraQ3.LockCam = true;
            GuiController.Instance.Drawer2D.beginDrawSprite();
            fondoMenu.render();
            instrucciones.render();
            GuiController.Instance.Drawer2D.endDrawSprite();
            if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.Back) || GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.M))
            {
                estadoJuego = estado.menu;
            }
        }

        private void menu(TgcD3dInput input)
        {
            camaraQ3.LockCam = true;
            GuiController.Instance.Drawer2D.beginDrawSprite();
            
            //Dibujo menu
            fondoMenu.render();
            titulo.render();
            botonJugar.render();
            botonInstrucciones.render();
            botonCreditos.render();
            
            
            //Hago click para empezar a jugar
            if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.J))
            {
                if (!primeraVez)
                {
                    barraVida.Scaling = new Vector2(0.3f, 0.3f);
                    velocidadEnemigos = -5f;
                    instanciasEnemigos.Clear();
                    instanciasEnemigos = new List<Enemigo>();
                    barriles.Clear();
                    //loader.loadAnimationFromFile(originalEnemigo, mediaPath + "\\Animations\\Walk-TgcSkeletalAnim.xml");
                    inicializarEnemigos(4, 4, instanciasEnemigos, 3.4f, 200.0f);
                    inicializarBarriles();
                }
                primeraVez = false;
                player.closeFile();
                player.play(true);
                puntaje = 0;
                numVida = 100;
                estadoJuego = estado.jugar;
            }
            if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.I))
            {
                estadoJuego = estado.instrucciones;
            }
            if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.C))
            {
                estadoJuego = estado.creditos;
            }
            GuiController.Instance.Drawer2D.endDrawSprite();
            textoPuntajeRecord.render();
        }

        private bool obtenerColisionConTexto(TgcText2d boton, Size tam)
        {
            Point mouse = Control.MousePosition;
            return mouse.X > boton.Position.X && mouse.X < (boton.Position.X + tam.Width*0.5f) && mouse.Y > boton.Position.Y && mouse.Y < (boton.Position.Y + tam.Height*0.5f);

        }

        private bool obtenerColisionConBoton(TgcSprite boton,Size tam)
        {
            Point mouse = Control.MousePosition;
            return mouse.X > boton.Position.X && mouse.X < (boton.Position.X + (tam.Width*0.5f)) && mouse.Y > boton.Position.Y && mouse.Y < (boton.Position.Y + (tam.Height*0.5f));

        }


        private void jugar(TgcD3dInput input, float t)
        {
            camaraQ3.LockCam = true;
            camaraQ3.updateCamera();

            
            // Cargar variables de shader, por ejemplo el tiempo transcurrido.
            effect.SetValue("time", time);            

            //El personaje es una caja, uso su bounding box para detectar colisiones
            personaje.Position = camaraQ3.getPosition();
            personaje.move(new Vector3(0f, -30f, 0f));
            //personaje.render();
            //personaje.BoundingBox.render();

            foreach (TgcBoundingBox item in obtenerListaZona(ultimaPosCamara))
            {
                if (TgcCollisionUtils.testAABBAABB(personaje.BoundingBox, item))
                {
                    camaraQ3.setPosition(ultimaPosCamara);
                    //camaraQ3.setCamera(ultimaPosCamara, camaraQ3.getLookAt() + ultimaPosCamara);
                }
            }

            if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.C))
            {
                velocidadEnemigos = velocidadEnemigos + 0.4f;
                if (velocidadEnemigos >= 0f)
                {
                    velocidadEnemigos = 0f;
                }
            }
            if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.V))
            {
                velocidadEnemigos = velocidadEnemigos - 0.4f;
            }

            ajustarZoom();
            ajustarVelocidad();

            renderizarBarriles(t);
            renderizarPiedras();

            //Renderizar suelo y skybox
            piso.render();
            //terreno.render();
            skyBox.render();

            //Emitir un disparo
            if (input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                posicionRayBala = camaraQ3.getPosition();
                unaBala.velocidadVectorial = camaraQ3.getLookAt() - camaraQ3.getPosition();
                unaBala.ray = new TgcRay(posicionRayBala, unaBala.velocidadVectorial);
                huboDisparo = true;
                disparo.SoundBuffer.SetCurrentPosition(0);
                disparo.play(false);
            }
            //Reviso si colisiono contra un barril si hubo disparo
            if (huboDisparo)
            {
                foreach (Barril b in barriles)
                {
                    if (!b.fueDisparado && TgcCollisionUtils.intersectRayAABB(unaBala.ray, b.mesh.BoundingBox, out col))
                    {
                        
                        boundingBarril = new TgcBoundingSphere(b.mesh.BoundingBox.calculateBoxCenter(), 400f);
                        disparoBarril = true;
                        barrilDisparado = b;
                        b.fueDisparado = true;
                        if (barriles.TrueForAll(fueDisparado))
                        {
                            reproducirSorpresa = true;
                        }
                        explosion.SoundBuffer.SetCurrentPosition(0);
                        explosion.play(false);
                        huboDisparo = false;
                        break;
                    }
                }
                if (barrilDisparado!=null)
                {
                    //barriles.Remove(barrilDisparado);
                    barrilDisparado = null;
                }
            }
            
            //Se dibuja siempre al principio, habria que hacer instancias y dibujarlas cada vez que se disparo a un barril en esa posicion
            

            //Renderizar original e instancias (no dibujo original, solo instancias)   
            foreach (Enemigo enemigo in instanciasEnemigos)
            {
                enemigo.meshEnemigo.animateAndRender();
                if (enemigo.estaVivo)
                {
                    rotarMesh(enemigo.meshEnemigo);
                    enemigo.meshEnemigo.moveOrientedY(velocidadEnemigos);
                    enemigo.ultimaPosicion = enemigo.meshEnemigo.Position;
                    foreach (Barril item in barriles)
                    {
                        if (TgcCollisionUtils.testAABBAABB(enemigo.meshEnemigo.BoundingBox, item.mesh.BoundingBox))
                        {
                            enemigo.meshEnemigo.rotateY(FastMath.PI_HALF);
                            enemigo.meshEnemigo.moveOrientedY(velocidadEnemigos);
                        }
                    }
                    foreach (TgcBoundingBox bb in obtenerListaZona(enemigo.meshEnemigo.Position))
                    {
                        if (TgcCollisionUtils.testAABBAABB(enemigo.meshEnemigo.BoundingBox, bb))
                        {
                            enemigo.meshEnemigo.rotateY(FastMath.PI_HALF);
                            enemigo.meshEnemigo.moveOrientedY(velocidadEnemigos);
                        }
                    }

                    //enemigo colisiona contra personaje
                    if(TgcCollisionUtils.classifyBoxBox(enemigo.meshEnemigo.BoundingBox,personaje.BoundingBox) != TgcCollisionUtils.BoxBoxResult.Afuera)
                    {
                        //golpe.SoundBuffer.SetCurrentPosition(0);
                        golpe.play(false);
                        enemigo.meshEnemigo.moveOrientedY(-velocidadEnemigos*5);
                        camaraQ3.setPosition(ultimaPosCamara);
                        numVida -= 20f * t;
                        barraVida.Scaling = new Vector2(0.3f*0.01f * numVida, 0.3f);
                        //Muerte del personaje
                        if (numVida<=0)
                        {
                            camaraQ3.LockCam = false;
                            muerte.SoundBuffer.SetCurrentPosition(0);
                            muerte.play(false);
                            player.stop();
                            //player.closeFile();
                            estadoJuego = estado.muerto;
                        }
                    }

                    if (huboDisparo || disparoBarril)
                    {
                        if (disparoBarril)
                        {
                            if (TgcCollisionUtils.testSphereAABB(boundingBarril,enemigo.meshEnemigo.BoundingBox))
                            {
                                matarEnemigo(enemigo);
                            }
                        }
                        if (TgcCollisionUtils.intersectRayAABB(unaBala.ray, enemigo.meshEnemigo.BoundingBox, out col))
                        {
                            Vector3 p = enemigo.meshEnemigo.BoundingBox.calculateBoxCenter();
                            p.Y = 0f;
                            TgcBoundingBox cuerpoChico = enemigo.meshEnemigo.BoundingBox.clone();
                            cuerpoChico.scaleTranslate(p, new Vector3(1.8f, 3.2f, 1.8f));
                            //se evalua si la bala dio contra el enemigo
                            if (TgcCollisionUtils.intersectRayAABB(unaBala.ray, cuerpoChico, out col))
                            {
                                TgcBoundingBox head = enemigo.meshEnemigo.BoundingBox.clone();
                                head.scaleTranslate(enemigo.meshEnemigo.BoundingBox.calculateBoxCenter() + new Vector3(0.0f, 60f, 5.0f), new Vector3(0.5f, 0.5f, 0.5f));
                                //se evalua si fue un headshot
                                if (TgcCollisionUtils.intersectRayAABB(unaBala.ray, head, out col))
                                {
                                    headshot.SoundBuffer.SetCurrentPosition(0);
                                    headshot.play(false);
                                    puntaje += 20; //el headshot me da 20 puntos mas que un disparo normal
                                }
                                matarEnemigo(enemigo);
                                huboDisparo = false;
                            }
                        }
                        
                    }
                }
                else
                {
                    enemigo.tiempo += t;
                    enemigo.efecto.SetValue("time", enemigo.tiempo);
                }
                
            }
            huboDisparo = false;
            disparoBarril = false;

            if (instanciasEnemigos.TrueForAll(estaMuerto))
            {
                if (puntaje > Int32.Parse(System.IO.File.ReadAllText(alumnoMediaFolder + "\\record.txt")))
                {
                    textoPuntajeFinal.Text = "Nuevo Record! Tu puntaje fue: " + puntaje.ToString();
                    System.IO.File.WriteAllText(alumnoMediaFolder + "\\record.txt", puntaje.ToString());
                    //textoPuntajeRecord.Text = "Tu puntaje record es: " + puntaje.ToString();
                }
                else
                {
                    textoPuntajeFinal.Text = "Tu puntaje fue: " + puntaje.ToString();
                }
                player.stop();
                camaraQ3.LockCam = false;
                ganador.SoundBuffer.SetCurrentPosition(0);
                ganador.play(false);
                player.stop();
                estadoJuego = estado.ganador;
                
            }

            //enemigoEffect.SetValue("time", time);

            //Renderizar instancias
            //renderizarTodosLosArboles();
            //renderizarPasto();
            qt.render(GuiController.Instance.Frustum, false);
            ultimaPosCamara = camaraQ3.getPosition();

            //DIBUJOS 2D
            renderSprites(input);
        }

        private static bool estaMuerto(Enemigo e)
        {
            return !e.estaVivo;
        }

        private void matarEnemigo(Enemigo enemigo)
        {
            enemigo.estaVivo = false;
            puntoDisparo.Position = col;
            puntoDisparo.render();
            puntaje += 20;
            enemigo.meshEnemigo.Effect = enemigo.efecto;
            enemigo.meshEnemigo.Technique = "disparoEnemigo";
            
            //disparoBarril = false;
        }

        private void renderSprites(TgcD3dInput input)
        {
            //Iniciar dibujado de todos los Sprites de la escena
            GuiController.Instance.Drawer2D.beginDrawSprite();

            //Dibujar sprite (si hubiese mas, deberian ir todos aqu�)
            barraVida.render();
            vida.Text = FastMath.Ceiling(numVida).ToString();
            vida.render();
            
            if (miraActivada)
            {
                mira_zoom.render();
            }
            else
            {
                mira.render();
                arma.render();
            }
            //DISPARO
            if (input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT) && !miraActivada)
            {
                arma.Position = posicionArmaDisparo;
                arma.render();
                arma.Position = posicionArmaOriginal;
                cantF = 16;
            }

            if (cantF > 0)
            {
                fuegoArma.updateAndRender();
                cantF--;
            }
            
            textoPuntaje.Text = "Puntos: " + puntaje;
            textoPuntaje.render();

            //Finalizar el dibujado de Sprites
            GuiController.Instance.Drawer2D.endDrawSprite();
        }

        private void ajustarVelocidad()
        {
            if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.LeftShift) && !miraActivada)
            {
                camaraQ3.MovementSpeed = velocidadCorrer;
            }
            else
            {
                camaraQ3.MovementSpeed = velocidadMov;
            }
        }

        private void ajustarZoom()
        {
            float ang = 0f;
            //float num = (float)GuiController.Instance.Modifiers.getValue("valorFloat");
            if (GuiController.Instance.D3dInput.buttonDown(TgcD3dInput.MouseButtons.BUTTON_RIGHT))
            {
                miraActivada = true;
                ruedita -= GuiController.Instance.D3dInput.WheelPos;
                ang = 15.0f + ruedita;
                if (ang > 45.0f)
                {
                    ang = 45.0f;
                }
                else if (ang < 2.0f)
                {
                    ang = 2.0f;
                }
                anguloFov = FastMath.ToRad(ang);
                ruedita = ang - 15.0f;
            }
            else
            {
                ruedita = 0f;
                miraActivada = false;
                anguloFov = FastMath.ToRad(45.0f);
            }

            GuiController.Instance.D3dDevice.Transform.Projection = Matrix.PerspectiveFovLH(anguloFov, aspectRatio, 1f, 50000f);
        }

        private void renderizarTodosLosArboles()
        {
            foreach (TgcMesh arbol in arboles)
            {
                arbol.render();
            }
        }

        private void renderizarPasto()
        {
            foreach (TgcMesh p in pasto)
            {
                p.render();
            }
        }

        private void renderizarBarriles(float t)
        {
            foreach (Barril b in barriles)
            {
                if(!b.fueDisparado)
                {
                    b.mesh.render();
                }else
                {
                    b.tiempo += t;
                    explosiones[barriles.IndexOf(b)].Effect.SetValue("time", b.tiempo);
                    explosiones[barriles.IndexOf(b)].render();
                }
            }
            if (barriles.TrueForAll(fueDisparado))
            {
                logoTgc.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(camaraQ3.getPosition()));
                logoTgc.render();
            }
            if (reproducirSorpresa)
            {
                sorpresa.SoundBuffer.SetCurrentPosition(0);
                sorpresa.play(false);
                puntaje += 500;
                reproducirSorpresa = false;
            }

        }

        private bool fueDisparado (Barril b)
        {
            return b.fueDisparado;
        }

        private void renderizarPiedras()
        {
            foreach (TgcMesh p in piedras)
            {
                p.render();
            }
        }  

        #region inicializaciones

        public void inicializarTerreno()
        {
            //Crear SkyBox
            skyBox = new TgcSkyBox();
            skyBox.Center = new Vector3(0, 500, 0);
            skyBox.Size = new Vector3(20000, 20000, 20000);
            string texturesPath = GuiController.Instance.AlumnoEjemplosMediaDir;//GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\SkyBox4\\";
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "//FullMoon//up.png");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "//FullMoon//down.png");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "//FullMoon//right.png");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "//FullMoon//left.png");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "//FullMoon//front.png");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "//FullMoon//back.png");
            skyBox.SkyEpsilon = 50f;
            skyBox.updateValues();

            TgcBox tb = TgcBox.fromSize(skyBox.Center, skyBox.Size);
            bbSkyBox = tb.BoundingBox.clone();

            //Crear piso
            TgcTexture pisoTexture = TgcTexture.createTexture(d3dDevice, GuiController.Instance.AlumnoEjemplosMediaDir + "\\pasto2.jpg");
            piso = TgcBox.fromSize(new Vector3(0, 0, 0), new Vector3(20000, 0, 20000), pisoTexture);
            piso.UVTiling = new Vector2(50, 50);
            piso.updateValues();

            //crear terreno
            terreno = new TgcSimpleTerrain();
            terreno.loadHeightmap(GuiController.Instance.ExamplesMediaDir + "Heighmaps\\Heightmap1.jpg", 350f, 1.6f,new Vector3(0f,0f,0f));
            terreno.loadTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "\\pasto2.jpg");
        }

        private void inicializarArboles()
        {
            string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosMediaDir;

            //Cargar modelo de palmera original
            TgcSceneLoader loader1 = new TgcSceneLoader();
            scene = loader1.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vegetacion\\Palmera\\Palmera-TgcScene.xml");
            palmeraOriginal = scene.Meshes[0];

            //Cargar Shader personalizado
            palmeraOriginal.Effect = effect;
            palmeraOriginal.Technique = "RenderScene";

            //Cargar modelo de otro arbol
            TgcSceneLoader loader2 = new TgcSceneLoader();
            scene = loader2.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vegetacion\\ArbolSelvatico2\\ArbolSelvatico2-TgcScene.xml");
            otroArbolOriginal = scene.Meshes[0];

            //Cargar Shader personalizado
            otroArbolOriginal.Effect = effect;
            otroArbolOriginal.Technique = "RenderScene";

            //Crear varias instancias del modelo original, pero sin volver a cargar el modelo entero cada vez
            int rows = 30;
            int cols = 10;
            //float offset = 500;
            BBZona1 = new List<TgcBoundingBox>();
            BBZona2 = new List<TgcBoundingBox>();
            BBZona3 = new List<TgcBoundingBox>();
            BBZona4 = new List<TgcBoundingBox>();
            arboles = new List<TgcMesh>();
            //bool moverFila=false;
            int i = 0;
            int j = 0;
            for (i = 0; i < rows; i++)
            {
                for (j = 0; j < cols; j++)
                {
                    //Crear instancia de modelo
                    TgcMesh instance = palmeraOriginal.createMeshInstance(palmeraOriginal.Name + i + "_" + j);
                    instance.Effect = effect;
                    instance.Technique = "RenderScene";
                    instance.AlphaBlendEnable = true;

                    //Desplazarlo
                    instance.move(rand.Next(-10000, 10000), 0, rand.Next(-10000, 10000));
                    instance.Scale = new Vector3(1.3f, 1.3f, 1.3f);
                    //instance.move(0f, terreno.HeightmapData[(int)(instance.Position.X/350f/64f),(int)(instance.Position.Z/350f/64f)] * 1.6f, 0f);
                    //instance.Position.Y = (float)terreno.HeightmapData.GetValue((int)(instance.Position.X / 350f), (int)(instance.Position.Z / 350f));
                    

                    arboles.Add(instance);
                    obtenerListaZona(instance.Position).Add(clonarBoundingBoxArbol(instance.BoundingBox));
                }
            }
            i = 0; j = 0;
            for (i = 0; i < rows; i++)
            {
                for (j = 0; j < cols; j++)
                {
                    //Crear instancia de modelo
                    TgcMesh instance2 = otroArbolOriginal.createMeshInstance(otroArbolOriginal.Name + i + "_" + j);
                    instance2.Effect = effect;
                    instance2.Technique = "RenderScene";
                    instance2.AlphaBlendEnable = true;

                    //Desplazarlo
                    instance2.move(rand.Next(-10000, 10000), 0, rand.Next(-10000, 10000));
                    instance2.Scale = new Vector3(10f, 10f, 10f);

                    arboles.Add(instance2);
                    obtenerListaZona(instance2.Position).Add(clonarBoundingBoxArbol(instance2.BoundingBox));
                }
            }
        }

        private void inicializarPiedras()
        {
            string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosMediaDir;

            //Cargar modelo de palmera original
            TgcSceneLoader loader1 = new TgcSceneLoader();
            scene = loader1.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vegetacion\\Roca\\Roca-TgcScene.xml");
            piedraOriginal = scene.Meshes[0];
            piedraOriginal.AlphaBlendEnable = true;

            //Crear varias instancias del modelo original, pero sin volver a cargar el modelo entero cada vez
            int rows = 30;
            int cols = 20;
            piedras = new List<TgcMesh>();
            //bool moverFila=false;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    //Crear instancia de modelo
                    TgcMesh instance = piedraOriginal.createMeshInstance(pastoOriginal.Name + i + "_" + j);
                    instance.AlphaBlendEnable = true;

                    //Desplazarlo
                    instance.move(rand.Next(-10000, 10000), 0, rand.Next(-10000, 10000));
                    instance.Scale = new Vector3(2f, 2f, 2f);

                    piedras.Add(instance);
                }
            }
        }

        private void inicializarPasto()
        {
            string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosMediaDir;

            //Cargar modelo de palmera original
            TgcSceneLoader loader1 = new TgcSceneLoader();
            scene = loader1.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vegetacion\\Pasto\\Pasto-TgcScene.xml");
            pastoOriginal = scene.Meshes[0];
            pastoOriginal.AlphaBlendEnable = true;

            //Cargar Shader personalizado
            pastoOriginal.Effect = effect;
            pastoOriginal.Technique = "VientoPasto";

            //Crear varias instancias del modelo original, pero sin volver a cargar el modelo entero cada vez
            int rows = 50;
            int cols = 30;
            pasto = new List<TgcMesh>();
            //bool moverFila=false;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    //Crear instancia de modelo
                    TgcMesh instance = pastoOriginal.createMeshInstance(pastoOriginal.Name + i + "_" + j);
                    instance.Effect = effect;
                    instance.Technique = "VientoPasto";
                    instance.AlphaBlendEnable = true;

                    //Desplazarlo
                    instance.move(rand.Next(-10000, 10000), 0, rand.Next(-10000, 10000));
                    instance.Scale = new Vector3(3f, 8f, 5f);

                    pasto.Add(instance);
                }
            }
        }

        private void inicializarBarriles()
        {
            string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosMediaDir;
            Random rand1 = new Random();
            //Cargar modelo de palmera original
            TgcSceneLoader loader1 = new TgcSceneLoader();
            scene = loader1.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Objetos\\BarrilPolvora\\BarrilPolvora-TgcScene.xml");
            barril = scene.Meshes[0];
            //barril.AlphaBlendEnable = true;

            //Crear varias instancias del modelo original, pero sin volver a cargar el modelo entero cada vez
            int rows = 2;
            int cols = 5;
            barriles = new List<Barril>();
            explosiones = new List<TgcMesh>();
            //bool moverFila=false;

            for (int i = 0; i < rows; i++) 
            {
                for (int j = 0; j < cols; j++)
                {
                    //Crear instancia de modelo
                    TgcMesh instance = barril.createMeshInstance(barril.Name + i + "_" + j);
                    instance.AlphaBlendEnable = true;
                    TgcMesh instanceExplosion = esferaExplosion.createMeshInstance(i + "_" + j);
                    instanceExplosion.AlphaBlendEnable = true;

                    //Desplazarlo
                    instance.move(rand1.Next(-5000, 5000), 0, rand1.Next(-5000, 5000));
                    instance.Scale = new Vector3(4f, 4f, 4f);

                    instanceExplosion.Position = instance.Position;
                    instanceExplosion.move(new Vector3(0f, 100f, 0f));
                    instanceExplosion.Scale = new Vector3(6f, 6f, 6f);

                    Effect fx = TgcShaders.loadEffect(GuiController.Instance.AlumnoEjemplosMediaDir + "Shaders\\explosion.fx");
                    instanceExplosion.Effect = fx;
                    instanceExplosion.Technique = "explosion";

                    Barril instanciaBarril = new Barril(instance);
                    barriles.Add(instanciaBarril);
                    explosiones.Add(instanceExplosion);
                }
            }
        }

        private TgcBoundingBox clonarBoundingBoxArbol(TgcBoundingBox bb)
        {
            TgcBoundingBox bbClon = bb.clone();
            Vector3 pos = bb.calculateBoxCenter();
            pos.Y = 0f;
            pos.Z -= 40f;
            pos.X -= 30f;
            bbClon.scaleTranslate(pos, new Vector3(0.3f, 1f, 0.3f));
            return bbClon;
        }


        public void inicializarEnemigos(int columnas, int filas, List<Enemigo> lista, float scale, float radio)
        {
            //Cargar malla original
            loader = new TgcSkeletalLoader();
            string pathMesh = GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\CS_Arctic-TgcSkeletalMesh.xml";
            mediaPath = GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\";
            originalEnemigo = loader.loadMeshFromFile(pathMesh, mediaPath);
            originalEnemigo.Scale = new Vector3(3.4f, 3.4f, 3.4f);

            //Agregar animaci�n a original
            loader.loadAnimationFromFile(originalEnemigo, mediaPath + "\\Animations\\Walk-TgcSkeletalAnim.xml");

            //Cargar Shader personalizado
            //meshOriginal.Effect = enemigoEffect;
            //meshOriginal.Technique = "RenderScene";
            float x = 0f;
            float z = 0f;
            //instanciasMalos = new List<TgcSkeletalMesh>();
            //int rowsRobot = 3;
            //int colsRobot = 3;
            for (int k = 0; k < filas; k++)
            {
                for (int q = 0; q < columnas; q++)
                {
                    //Crear instancia de modelo
                    TgcSkeletalMesh instance = originalEnemigo.createMeshInstance(originalEnemigo.Name + k + "_" + q);
                    do
                    {
                        x = rand.Next(-5000, 5000);
                        z = rand.Next(-5000, 5000);
                    } while (FastMath.Sqrt(x * x + z * z) <= radio);
                    //Achico el bounding box del arbol
                    //instance.BoundingBox.scaleTranslate(new Vector3(0f, 0f, 0f), new Vector3(0.5f, 0.5f, 0.5f));


                    //instance.rotateY(FastMath.Atan(3f));
                    //Desplazarlo
                    instance.move(x, 0, z);

                    //Roto el mesh
                    rotarMesh(instance);

                    Effect fx = TgcShaders.loadEffect(GuiController.Instance.AlumnoEjemplosMediaDir + "Shaders\\e.fx");
                    

                    instance.Scale = new Vector3(scale, scale, scale);
                    instance.AlphaBlendEnable = true;
                    Enemigo e = new Enemigo(instance, instance.Position);
                    e.efecto = fx;
                    lista.Add(e);
                }
            }

            //originalEnemigo.playAnimation("Walk");
            foreach (Enemigo enemigo in lista)
            {
                enemigo.meshEnemigo.playAnimation("Walk");
            }

        }

        #endregion

        public void rotarMesh(TgcSkeletalMesh mesh1)
        {
            Vector3 haciaDondeDebeMirar;
            haciaDondeDebeMirar = mesh1.Position - camaraQ3.getPosition();
            haciaDondeDebeMirar.Y = 0;
            haciaDondeDebeMirar.Normalize();
            mesh1.rotateY((float)FastMath.Atan2(haciaDondeDebeMirar.X, haciaDondeDebeMirar.Z) - mesh1.Rotation.Y);
        }

        public void orientarBala(TgcBox bala)
        {
            Vector3 haciaDondeDebeMirar = camaraQ3.getPosition() - camaraQ3.getLookAt();
            haciaDondeDebeMirar.Y = 0;
            haciaDondeDebeMirar.Normalize();
            bala.rotateY((float)FastMath.Atan2(haciaDondeDebeMirar.X, haciaDondeDebeMirar.Z) - bala.Rotation.Y);
        }

        #region optimizaciones
        public int obtenerZona(Vector3 pos)
        {
            if (pos.X <= 0)
            {
                if (pos.Z <= 0)
                {
                    return 1;
                }
                else
                {
                    return 2;
                }
            }
            else
            {
                if (pos.Z <= 0)
                {
                    return 3;
                }
                else
                {
                    return 4;
                }
            }
        }

        private List<TgcBoundingBox> obtenerListaZona(Vector3 pos)
        {
            switch (obtenerZona(pos))
            {
                case 1:
                    return BBZona1;
                case 2:
                    return BBZona2;
                case 3:
                    return BBZona3;
                case 4:
                    return BBZona4;
                default:
                    return null;
            }
        }
        #endregion

        /// <summary>
        /// M�todo que se llama cuando termina la ejecuci�n del ejemplo.
        /// Hacer dispose() de todos los objetos creados.
        /// </summary>
        public override void close()
        {
            //enemigoEffect.Dispose();
            effect.Dispose();
            originalEnemigo.dispose();
        }


    }
}
