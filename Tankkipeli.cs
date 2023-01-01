using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;

public class Tankkipeli : PhysicsGame
{
    PhysicsObject tankki1;
    PhysicsObject tankki2;




    private static readonly String[] lines = {

                  "                        ",
                  "      XXX        X      ",
                  "                 X      ",
                  "                 X      ",
                  "                        ",
                  "        X     XX        ",
                  "        X     XX        ",
                  "        X     XX        ",
                  "                        ",
                  "         XXX            ",
                  };
    private static readonly int tileWidth = 500 / lines.Length;
    private static readonly int tileHeight = 700 / lines.Length;


    public override void Begin()
    {
        ClearAll();
        AlkuValikko();
    }


    private void AlkuValikko()
    {
        MultiSelectWindow alkuvalikko = new MultiSelectWindow("Pelin alkuvalikko", "Aloita peli", "Lopeta");
        Add(alkuvalikko);
        alkuvalikko.AddItemHandler(0, AloitaPeli);
        alkuvalikko.AddItemHandler(1, Exit);
    }


    private void AloitaPeli()
    {
        LuoKentta();
        AsetaOhjaimet();
    }

    
    private void LuoKentta()
    {
        tankki1 = LuoTankki(Level.Right - 20.0, 0.0);
        tankki2 = LuoTankki(Level.Left + 20.0, 0.0);


        Vector [] miinat = {new Vector(500, 300), new Vector(500, -300), new Vector(0, 0), new Vector(-500, 300), new Vector(-500, 300)};
        LuoMiina(miinat);


        TileMap tiles = TileMap.FromStringArray(lines);
        tiles.SetTileMethod('X', LuoSeina, Color.DarkForestGreen);
        tiles.Execute(tileWidth, tileHeight);


        PhysicsObject vasenReuna = Level.CreateLeftBorder();
        vasenReuna.Image = LoadImage("seina1");
        vasenReuna.Restitution = 1.0;
        vasenReuna.IsVisible = true;

        PhysicsObject oikeaReuna = Level.CreateRightBorder();
        oikeaReuna.Image = LoadImage("seina1");
        oikeaReuna.Restitution = 1.0;
        oikeaReuna.IsVisible = true;

        PhysicsObject ylaReuna = Level.CreateTopBorder();
        ylaReuna.Image = LoadImage("seina");
        ylaReuna.Restitution = 1.0;
        ylaReuna.IsVisible = true;

        PhysicsObject alaReuna = Level.CreateBottomBorder();
        alaReuna.Image = LoadImage("seina");
        alaReuna.Restitution = 1.0;
        alaReuna.IsVisible = true;

        Level.Background.Color = Color.AshGray;

        Camera.ZoomToLevel(100);
    }


    /// <summary>
    /// Luodaan pelikentän kiinteät rakenteet ja niiden ulkonäkö.
    /// </summary>
    /// <param name="paikka">seinän sijainti</param>
    /// <param name="leveys">seinän leveys</param>
    /// <param name="korkeus">seinän korkeus</param>
    /// <param name="vari"></param>
    private void LuoSeina(Vector paikka, double leveys, double korkeus, Color vari)
    {
        PhysicsObject seina = new PhysicsObject(leveys - 1, korkeus - 2);
        seina.Position = paikka;
        seina.Color = vari;
        seina.Tag = "rakenne";
        seina.MakeStatic();
        seina.Image = LoadImage("tiili");
        Add(seina);
    }


    /// <summary>
    /// Luodaan pelaajien tankin koko, muoto ym ominaisuudet, annetaan tankille tykki lapsioliona. 
    /// </summary>
    /// <param name="x">tankin x-koordinaatti</param>
    /// <param name="y">tankin y-koordinaatti</param>
    /// <returns>
    /// Palauttaa pelaajien tankit.
    /// </returns>
    private PhysicsObject LuoTankki(double x, double y)
    {
        PhysicsObject tankki = new PhysicsObject(50.0, 50.0);
        tankki.Shape = Shape.Rectangle;
        tankki.X = x;
        tankki.Y = y;
        tankki.CanRotate = false;
        tankki.KineticFriction = 1;
        Cannon tykki = LuoTykki(x, y);
        tankki.Add (tykki);
        tankki.Image = LoadImage("esimerkkitankki");
        tankki.Tag = "tankki";
        AddCollisionHandler(tankki, "tankki", PelaajatTormaavat);
        AddCollisionHandler(tankki, "miina", MiinaOsui);
        Add(tankki);
        return(tankki);
    }


    /// <summary>
    /// Luodaan tankkien tykit Jypelin asetoiminnon avulla.
    /// </summary>
    /// <param name="x">tankin aseen x-koordinaatti</param>
    /// <param name="y">tankin aseen y-koordinaatti</param>
    /// <returns>
    /// Palauttaa tankeille niiden aseet.
    /// </returns>
    private Cannon LuoTykki(double x, double y)
    {
        Cannon tankkiAse = new Cannon(50, 10);
        tankkiAse.X = x;
        tankkiAse.Y = y;
        tankkiAse.FireRate = 0.5;
        tankkiAse.ProjectileCollision = AmmusOsui;

        return tankkiAse;
    }


    /// <summary>
    /// Luodaan miinojen koko, muoto, liikumattomuus ja kuva.
    /// </summary>
    /// <param name="miinat">miinojen vektorin pisteet</param>
    /// <returns>
    /// Palauttaa miinan.
    /// </returns>
    private void LuoMiina(Vector [] miinat)
    {
        for (int i = 0; i < miinat.Length; i++)
        {
            PhysicsObject miina = new PhysicsObject(30.0, 30.0);
            miina.Shape = Shape.Circle;
            miina.MakeStatic();
            miina.Tag = "miina";
            miina.Image = LoadImage("miina");
            miina.Position =  miinat [i];
            Add(miina);
        }
    }


    /// <summary>
    /// Mahdollistetaan tankille pyöriminen myötä- ja vastapäivään
    /// </summary>
    /// <param name="tankki">pelaajan tankki</param>
    /// <param name="suunta">myötä- tai vastapäivään pyörittäminen</param>
    /// <param name="nopeus">millä nopeudella tankki pyörii</param>
    private void Pyorita(PhysicsObject tankki, int suunta, int nopeus)
    {
        Angle l = new Angle();
        l.Degrees = nopeus * suunta;
        tankki.Angle += l;
    }


    /// <summary>
    /// Asetetaan peliin ohjaimet, millä tankkeja liikutellaan, ja miten niillä ammutaan.
    /// </summary>
    private void AsetaOhjaimet()
    {
        Keyboard.Listen(Key.Up, ButtonState.Down, LiikutaTankkia, "Liikuta tankkia eteenpäin", tankki1, 100.0 );
        Keyboard.Listen(Key.Up, ButtonState.Released, AsetaNopeus, "Liikuta tankkia eteenpäin", tankki1, Vector.Zero);
        Keyboard.Listen(Key.Left, ButtonState.Down, Pyorita, "Liikuta tankkia vasemmalle", tankki1, 1, 5);
        Keyboard.Listen(Key.Down, ButtonState.Down, LiikutaTankkia, "Liikuta tankkia taaksepäin", tankki1, -100.0);
        Keyboard.Listen(Key.Down, ButtonState.Released, AsetaNopeus, "Liikuta tankkia taaksepäin", tankki1, Vector.Zero);
        Keyboard.Listen(Key.Right, ButtonState.Down, Pyorita, "Liikuta tankkia oikealle", tankki1, -1, 5);
        Keyboard.Listen(Key.Space, ButtonState.Down, AmmuAseella, "Ammu", tankki1);

        Keyboard.Listen(Key.W, ButtonState.Down, LiikutaTankkia, "Liikuta tankkia eteenpäin", tankki2, 100.0);
        Keyboard.Listen(Key.W, ButtonState.Released, AsetaNopeus, "Liikuta tankkia eteenpäin", tankki2, Vector.Zero);
        Keyboard.Listen(Key.A, ButtonState.Down, Pyorita, "Liikuta tankkia vasemmalle", tankki2, 1, 5);
        Keyboard.Listen(Key.S, ButtonState.Down, LiikutaTankkia, "Liikuta tankkia taaksepäin", tankki2, -100.0);
        Keyboard.Listen(Key.S, ButtonState.Released, AsetaNopeus, "Liikuta tankkia taaksepäin", tankki2, Vector.Zero);
        Keyboard.Listen(Key.D, ButtonState.Down, Pyorita, "Liikuta tankkia oikealle", tankki2, -1, 5);
        Keyboard.Listen(Key.Q, ButtonState.Down, AmmuAseella, "Ammu", tankki2);
    }


    /// <summary>
    /// Asetetaan tankin liikkumisnopeus.
    /// </summary>
    /// <param name="tankki">pelaajan tankki</param>
    /// <param name="nopeus">millä nopeudella tankki liikkuu</param>
    private void AsetaNopeus(PhysicsObject tankki, Vector nopeus)
    {
        tankki.Velocity = nopeus;
    }


    /// <summary>
    /// Esitetään, mitä tapahtuu pelaajien törmätessä toisiinsa.
    /// </summary>
    /// <param name="tormaaja">törmäävä tankki</param>
    /// <param name="kohde">törmäyksen vastaanottava tankki</param>
    private void PelaajatTormaavat(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        MessageDisplay.Add("Bump!");
    }


    /// <summary>
    /// Ilmoittaa mihin suuntaan tankki milläkin hetkellä osoittaa.
    /// </summary>
    /// <param name="tankki">pelaajan tankki</param>
    /// <returns>
    /// Pelaajan suunnan.
    /// </returns>
    private Vector TankinSuunta(PhysicsObject tankki)
    {
        double x = tankki.Angle.Cos;
        double y = tankki.Angle.Sin;
        Vector suunta = new Vector(x, y);
        return suunta.Normalize();
    }


    /// <summary>
    /// Määrittää tankin liikkumisen siihen suuntaan, mihin se osoittaa
    /// </summary>
    /// <param name="tankki">pelaajan tankki</param>
    /// <param name="nopeus">millä nopeudella tankki liikkuu</param>
    void LiikutaTankkia(PhysicsObject tankki, double nopeus)
    {
        Vector suunta = TankinSuunta(tankki);
        tankki.Move(suunta * nopeus);
    }


    /// <summary>
    /// Määrittää, mitä tapahtuu ammuksen osuessa pelaajaan.
    /// </summary>
    /// <param name="ammus">tykin panos</param>
    /// <param name="kohde">pelaajan tankki</param>
    private void AmmusOsui(PhysicsObject ammus, PhysicsObject kohde)
    {


        ammus.Destroy();

        if (kohde == tankki1 || kohde == tankki2)
        {
            TankkiKuoli(kohde);
        }

    }


    /// <summary>
    /// Määrittää, mitä tapahtuu pelaajan osuessa miinaan
    /// </summary>
    /// <param name="tankki">pelaajan tankki</param>
    /// <param name="miina">kentällä oleva miina</param>
    private void MiinaOsui(PhysicsObject tankki, PhysicsObject miina)
    {
        miina.Destroy();
        TankkiKuoli(tankki);
       
    }


    private void TankkiKuoli (PhysicsObject tankki)
    {

        Explosion rajahdys = new Explosion(80);
        rajahdys.Position = tankki.Position;
        Add(rajahdys);
        tankki.Destroy();
        LopetaPeli();
    }


    private void LopetaPeli()
    {
        MessageDisplay.Add("BAM! Peli päättyi");
        Timer.SingleShot(3, Begin);
    }


    /// <summary>
    /// Mahdollistaa tankin tykillä ampumisen.
    /// </summary>
    /// <param name="tankki">pelaajan tankki</param>
    private void AmmuAseella(PhysicsObject tankki)
    {
        IEnumerable<Cannon> cannons = tankki.GetChildObjects<Cannon>();
        foreach (Cannon cannon in cannons)
        {
           
            PhysicsObject ammus = cannon.Shoot();

            if (ammus != null)
            {
                ammus.Size *= 1;
                ammus.MaximumLifetime = TimeSpan.FromSeconds(2.0);
            }
        }
        
    }
}

  