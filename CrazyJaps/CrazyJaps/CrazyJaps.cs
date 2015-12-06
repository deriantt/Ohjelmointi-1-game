using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;


/// @author  Deniz Anttila
/// @version 22.12.2011
/// <summary>
/// Peli, jossa toimitaan Kamikaze lentäjänä 
/// toisessa maailmansodassa ja koitetaan tuhota Yhdysvaltojen
/// laivoja
/// </summary>
/// <remarks>
/// Todo:
///  - Viritä lentäjän ase kuntoon
/// </remarks>
/// 
public class CrazyJaps : PhysicsGame
{
    private static String[] taso1 = {
                  "p          ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "    *      ",
                  "XXXXXXXXXXXX",
                  };

    private static String[] taso2 = {
                  "p          ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "           ",
                  "     * *   ",
                  "XXXXXXXXXXXX",
                  };

    private static string[][] tasolista = { taso1, taso2 };
    private int tileWidth;
    private int tileHeight;

    private IntMeter tasoNr = new IntMeter(0, 0, 10);
    private readonly Image JapOikea = LoadImage("JapOikea");
    private readonly Image JapVasen = LoadImage("JapVasen");
    private readonly Image JapAlas = LoadImage("JapAlas");
    private readonly Image ShipImage = LoadImage("Ship");
    private readonly Image ammusImage = LoadImage("ammus");
    private readonly Image vesiImage = LoadImage("vesi");
    private readonly Image taustaKuva = LoadImage("Newspaper");
    private readonly Image taustaKuva2 = LoadImage("JapAttack");
    private readonly Image ShipSink = LoadImage("SinkingShip");

    private IntMeter Ships = new IntMeter(0);
    private List<Label> valikonKohdat;
    private PhysicsObject player;

    //Aloitetaan peli alkuvalikolla
    public override void Begin()
    {


        Valikko();
        Level.Background.CreateGradient(Color.Black, Color.Black);
        Level.Background.Image = taustaKuva;
        //Level.Background.ScaleToLevelFull();
        Level.Background.ScaleToLevelByWidth();
    }


    // Pelin valikko jossa 
    //vaihtoehtoina Start Game tai Quit
    void Valikko()
    {
        ClearAll();
        MediaPlayer.Play("EpicMusic");
        Level.Background.CreateGradient(Color.Black, Color.Black);
        Level.Background.Image = taustaKuva2;
        Level.Background.ScaleToLevelByWidth();
        valikonKohdat = new List<Label>();

        Label kohta1 = new Label("Start New Mission");
        kohta1.Position = new Vector(0, 40);
        valikonKohdat.Add(kohta1);// lisää "start new mission" valikon kohtiin

        Label kohta2 = new Label("Quit");
        kohta2.Position = new Vector(0, -40);
        valikonKohdat.Add(kohta2);

        foreach (Label valikonKohta in valikonKohdat)
        {
            Add(valikonKohta);
        }

        Mouse.ListenOn(kohta1, MouseButton.Left, ButtonState.Pressed, AloitaUusiPeli, null);
        Mouse.ListenOn(kohta2, MouseButton.Left, ButtonState.Pressed, Exit, null);

        Mouse.IsCursorVisible = true;
        Mouse.ListenMovement(1.0, ValikossaLiikkuminen, null);
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, Exit, "");

    }


    /// <summary>
    /// Valikon liikkumisen asettaminen
    /// </summary>
    void ValikossaLiikkuminen(AnalogState hiirenTila)
    {
        foreach (Label kohta in valikonKohdat)
        {
            if (Mouse.IsCursorOn(kohta))
            {
                kohta.TextColor = Color.Red;
            }
            else
            {
                kohta.TextColor = Color.Black;
            }

        }
    }


    /// <summary>
    /// Aloitetaan peli ja myös uusitaan peli.
    /// </summary>
    public void AloitaUusiPeli()
    {

        ClearControls();
        ClearAll();
        tasoNr.Value = 0;
        // Tähän tulee kaikki kentän luomiset ym. alustukset...
        MediaPlayer.Play("Tausta");
        LuoKentta();

    }


    /// <summary>
    /// Luodaan pelikenttä.
    /// </summary>
    public void LuoKentta()
    {
        ClearGameObjects();
        ClearControls();

        System.GC.Collect();
        Ships.Value = 0;

        tasoNr.Value++;
        if (tasoNr.Value > tasolista.Length)// ei enempää tasoja niin loppuu peli
        {
            GameEnd();

            return;
        }
        ClearAll();

        int index = tasoNr.Value - 1;
        string[] tasonKuva = tasolista[index];

        tileWidth = 800 / tasonKuva[0].Length;
        tileHeight = 480 / tasonKuva.Length;
        TileMap tiles = TileMap.FromStringArray(tasonKuva);

        Gravity = new Vector(0, -20);
        IsFullScreen = true;

        tiles['X'] = CreateSea;
        tiles['*'] = CreateShip;
        tiles['p'] = CreatePlayer;
        tiles.Insert(tileWidth, tileHeight);



        Surface pohja = Surface.CreateBottom(Level);
        Add(pohja);
        //Level.CreateBorders();
        Level.Background.CreateGradient(Color.White, Color.LightBlue);
        Camera.ZoomToLevel(0);
        //AsetaOhjaimet();


    }


    /// <summary>
    /// NaytaIlmoitus on aliohjelma loppu ilmoituksen luomiseen
    /// </summary>
    private void NaytaIlmoitus(string teksti, Color vari)
    {

        Label label = new Label(teksti);
        label.Font = Font.DefaultLargeBold;
        label.Width = 600;
        label.Height = 200;
        label.Color = vari;
        label.BorderColor = Color.Black;
        Add(label);
        ClearControls();
        AsetaJatkoOhjaimet(label);
;
    }


    /// <summary>
    /// Pelin loppumisen jälkeen asetetaan jatko-ohjaimet.
    /// </summary>
    private void AsetaJatkoOhjaimet(Label teksti)
    {
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Show help");
        Keyboard.Listen(Key.F5, ButtonState.Pressed, Begin, "New game");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, Exit, "Exit game");

        //Mouse.ListenOn(pisteNaytto, MouseButton.Left, ButtonState.Pressed, delegate() { AloitaUusiPeli(); }, null);
        Mouse.ListenOn(teksti, MouseButton.Left, ButtonState.Pressed, delegate() { AloitaUusiPeli(); }, null);
    }


    /// <summary>
    /// Luodaan pelaaja joka on siis lentokone jonka päämäärä
    /// on tuhota laiva, joko lentämällä siihen tai ampumalla
    /// </summary>
    /// <returns>luotu pelaaja</returns>
    public PhysicsObject CreatePlayer()
    {

        player = new PhysicsObject(30, 30, Shape.Rectangle);

        player.Image = JapOikea;
        player.Tag = "player";
        player.Acceleration = new Vector(70, 0);
        player.Velocity = new Vector(95, 0);
        player.CanRotate = false;
        player.IgnoresExplosions = true;

        AssaultRifle pyssy = new AssaultRifle(1, 1);
        pyssy.Ammo.Value = 500;
        player.Add(pyssy);
        pyssy.Angle += Angle.FromDegrees(0);
        Keyboard.Listen(Key.Space, ButtonState.Down, delegate { AmmuAseella(pyssy); }, "Ammu");
        pyssy.ProjectileCollision = KuulaOsuu;
        AsetaOhjaimet(player);

        return player;

    }


    /// <summary>
    /// Luodaan uusi pelaaja alkuperäisen tuhouduttua
    /// </summary>
    public void LuoUusiPlayer()
    {
        TileMap tiles = TileMap.FromStringArray(taso1);
        tiles['p'] = CreatePlayer;
        tiles.Insert(tileWidth, tileHeight);
    }


    /// <summary>
    /// Pelaajan aseen ampuminen
    /// </summary>
    /// <param name="pyssy">pelaajan ase</param>
    void AmmuAseella(AssaultRifle pyssy)
    {
        PhysicsObject ammus = pyssy.Shoot();

        if (ammus != null)
        {
            ammus.Size *= 1;
        }
    }


    /// <summary>
    /// Asetetaan pelaajalle ohjaimet.
    /// </summary>
    /// <param name="player">pelaaja</param>
    private void AsetaOhjaimet(PhysicsObject player)
    {
        Keyboard.Listen(Key.Left, ButtonState.Pressed, MovePlayer, "Move Left", player, new Vector(-90, 0));
        Keyboard.Listen(Key.Right, ButtonState.Pressed, MovePlayer, "Move Right", player, new Vector(90, 0));
        Keyboard.Listen(Key.Up, ButtonState.Pressed, MovePlayer, "Move Up", player, new Vector(0, 50));
        Keyboard.Listen(Key.Down, ButtonState.Pressed, MovePlayer, "Move Down", player, new Vector(0, -100));
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, null);
        Keyboard.Listen(Key.F5, ButtonState.Pressed, Begin, "New Game");
        //Keyboard.Listen(Key.Space, ButtonState.Down, AmmuAseella, "Ammu");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, Valikko, "Palaa valikkoon");

        Mouse.IsCursorVisible = false;
    }


    /// <summary>
    /// Aliohjelma pelaajan liikkumista varten
    /// </summary>
    /// <param name="player">pelaaja</param>
    /// <param name="force">Arvot liikkumista varten</param>
    void MovePlayer(PhysicsObject player, Vector force)
    {

        player.Move(force);

        Vector suuntaVasen = new Vector(-90, 0);
        Vector suuntaOikea = new Vector(90, 0);
        Vector suuntaAlas = new Vector(0, -100);
        if (force == suuntaVasen)
        {
            player.Image = JapVasen;
            //player.Angle += Angle.FromDegrees(0);
        }
        if (force == suuntaOikea)
        {
            player.Image = JapOikea;
            //player.Angle += Angle.FromDegrees(-90);
        }
        if (force == suuntaAlas) player.Image = JapAlas;
    }


    /// <summary>
    /// Luodaan merielementti. Meri toimii myös pelin pohjatasona ja
    /// pelaajan osuessa siihen lentokone tuhoutuu.
    /// </summary>
    /// <returns>luotu meri</returns>
    private PhysicsObject CreateSea()
    {
        PhysicsObject sea = PhysicsObject.CreateStaticObject(tileWidth, tileHeight);
        sea.Color = Color.Blue;
        sea.CollisionIgnoreGroup = 1;
        AddCollisionHandler(sea, CollisionWithSea);

        return sea;
    }


    /// <summary>
    /// Luodaan laiva. Laiva toimii myös pelaajan vihollisena ja
    /// pelaajan osuessa siihen se ja lentokone tuhoutuu.
    /// Pelaaja voi myös tuhota laivan ampumalla.
    /// </summary>
    /// <returns>luotu laiva</returns>
    private PhysicsObject CreateShip()
    {
        PhysicsObject ship = new PhysicsObject(tileWidth + 10, tileHeight);
        ship.Image = ShipImage;
        ship.Tag = "ship";
        ship.CollisionIgnoreGroup = 1;
        ship.IgnoresGravity = true;
        ship.IgnoresExplosions = true;
        //----------------------Ampuminen----------------------

        Cannon tykki = new Cannon(30, 10);
        tykki.Tag = "tykki";
        tykki.Power.DefaultValue = 3000;

        tykki.Angle += Angle.FromDegrees(80);
        tykki.ProjectileCollision = KuulaOsuu;


        ship.Add(tykki);

        AddCollisionHandler(ship, CollisionWithShip);

        Timer TykkiAjastin = new Timer();
        TykkiAjastin.Interval = 3.5;
        TykkiAjastin.Timeout += delegate { ShipAmpuu(tykki); };
        TykkiAjastin.Start(100);

        Ships.Value++;
        
        ship.Destroyed += delegate { Ships.Value--; };
        if (Ships.Value == 1) ship.Destroyed += delegate { TasoPelattu(); };
        

        return ship;
    }


    /// <summary>
    /// Apualiohjelma laivan tykin ampumiseksi lentokonetta kohti
    /// </summary>
    /// <param name="tykki">Laivan ampuva tykki</param>
    void ShipAmpuu(Cannon tykki)
    {
        Vector suunta = -(tykki.AbsolutePosition - player.Position).Normalize();

        tykki.Angle = suunta.Angle;

        PhysicsObject ammus = tykki.Shoot();
        ammus.IgnoresExplosions = true;
        ammus.Image = ammusImage;

        ammus = tykki.Shoot();

        //      Timer airAjastin = new Timer();
        //      airAjastin.Interval = 2;
        //      airAjastin.Timeout += delegate { airGrenade(ammus); };
        //      airAjastin.Start(10);

        if (ammus != null)
        {
            ammus.Size *= 3;
            ammus.Velocity = new Vector(ammus.Velocity.X * 1.5, ammus.Velocity.Y * 1.5);
            ammus.Width = 1;
            ammus.Height = 1;
        }

    }


    /// <summary>
    /// Vesi loiskuu kun laiva tuhoutuu
    /// </summary>
    /// <param name="ship">Laiva</param>
    private void Liekita(IPhysicsObject ship)
    {
        Flame liekki = new Flame(vesiImage);
        liekki.MaximumLifetime = TimeSpan.FromSeconds(3);
        liekki.Position = ship.Position;
        liekki.MinVelocity = 5;
        liekki.MaxVelocity = 30;
        Add(liekki);
        Timer timer = new Timer();
        timer.Interval = 2;
        timer.Start();
        timer.Timeout += delegate()
        {
            liekki.Destroy();
            timer.Stop();
        };
    }


    //Jätetään tässä versiossa pois
    //  void airGrenade(PhysicsObject ammus)
    //  {
    //      ammus.Destroy();
    //      Explosion pum = new Explosion(100);
    //      pum.Position = ammus.Position;
    //      Add(pum);
    //  }


    /// <summary>
    /// Apualiohjelma laivan ja siihen lentokoneen räjäyttämiseksi ja poistamiseksi
    /// </summary>
    /// <param name="Ship">olio joka räjäytetään</param>
    /// <param name="target">törmäävä olio</param>
    void CollisionWithShip(PhysicsObject ship, PhysicsObject target)
    {
        ship.IgnoresGravity = true;
        ship.Destroy();
        //Ships.Value--;
        Liekita(ship);

        Add(Rajahdys(100, ship));
        Savuta(ship);

        if (target == player)
        {
            LuoUusiPlayer();
        }

        target.Destroy();


    }


    /// <summary>
    /// Apualiohjelma lentokoneen räjäyttämiseksi ja poistamiseksi 
    /// törmätessään mereen.
    /// </summary>
    /// <param name="Ship">olio joka räjäytetään</param>
    /// <param name="target">törmäävä olio</param>
    void CollisionWithSea(PhysicsObject sea, PhysicsObject target)
    {


        target.Destroy();
        Add(Rajahdys(100, target));

        if (target == player)
        {
            LuoUusiPlayer();
        }
    }


    /// <summary>
    /// Kun laivatykin ampuma ammus osuu lentokoneeseen
    /// niin kone räjähtää ja luodaan uusi pelaaja
    /// </summary>
    /// <param name="kohde">tuhotaan</param>
    /// <param name="ammus">tykin ammus</param>
    void KuulaOsuu(PhysicsObject ammus, PhysicsObject kohde)
    {
        ammus.Destroy();
        if (kohde == player) kohde.Destroy();

        Add(Rajahdys(100, ammus));

        if (kohde == player)
        {
            LuoUusiPlayer();
        }
    }


    /// <summary>
    /// Tehdään räjähdys jota voi käyttää useammassa tilanteessa
    /// </summary>
    /// <returns>luotu räjähdys</returns>
    /// <param name="koko">räjähdyksen koko</param>
    /// <param name="target">olio joka räjähtää</param>
    public Explosion Rajahdys(double koko, PhysicsObject target)
    {
        Explosion rajahdys = new Explosion(koko);
        rajahdys.Position = target.Position;
        rajahdys.Speed = 500.0;
        rajahdys.Force = 100;
        rajahdys.ShockwaveColor = Color.LightYellow;
        return rajahdys;
    }


    /// <summary>
    /// Kun lentokone osuu laivaan
    /// niin laivasta jää vain savu pilvi
    /// </summary>
    /// <param name="kohde">tuhotaan</param>
    /// <param name="ammus">tykin ammus</param>
    private void Savuta(IPhysicsObject olio)
    {
        Smoke savu = new Smoke();
        savu.MaximumLifetime = TimeSpan.FromSeconds(8);
        savu.Position = olio.Position;
        Add(savu);
        Timer timer = new Timer();
        timer.Interval = 3;
        timer.Start();
        timer.Timeout += delegate()
        {
            savu.Destroy();
            timer.Stop();
        };
    }


    /// <summary>
    /// Taso on valmis, kaikki pallot on käytetty.
    /// </summary>
    private void TasoPelattu()
    {
         //int Ships = GetObjects(olio => (string)(olio.Tag) == "Ship").Count;
        if (Ships.Value <= 0)
        {
            Timer.SingleShot(3.0, LuoKentta);
            return;
        }

    }


    /// <summary>
    /// Pelin loppu kuva ja ilmoitus
    /// </summary>
    void GameEnd()
    {
        ClearAll();
        Level.Background.CreateGradient(Color.Black, Color.Black);
        Level.Background.Image = ShipSink;
        Level.Background.ScaleToLevelByWidth();
        NaytaIlmoitus("You are honourabre Japanese warrior!", Color.White);

        Timer.SingleShot(8.0, Valikko);

    }
}