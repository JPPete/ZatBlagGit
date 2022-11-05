# ZatBlagGit

Program for updating fields in a SQL database

Dokumentacija programa ZatBlag

Program “ZatBlag” je napravljen sa svrhom da se resi problem kada ne mogu da se zatvore blagajne iz BBIS-a I moraju da se “zatvore iz baze”. Ovaj program treba da omoguci svim korisnicima to da urade bez pristupa serveru I znanja SQL-a. Program je napravljen koriscenjem XAML I C# . Pri pokretanju programa mozemo da vidimo sledece:

Pocetni user interface sadrzi:

Naslov
Polje za unos broja poslovnice
DatePicker za unos\odabir datuma
Polje za unos broja blagajne
Label statusa/obavestenja (default: “Proveri da li su zatvorene blagajne”)
DataGrid gde se prikazuju podaci iz baze
Dugme “Provera” Polja za unos broja poslovnice I broja blagajne sadrze “Input Validation” koji ogranicava da mogu samo da se unose brojevi (0-9) u ta polja. Polje za unos broja poslovnice dalje ogranicava unos do 6 brojeva, dok polje za unos blagajne ogranicava na 4 broja.
Pritiskom na dugme “Provera” pokrece se:

Provera da li je ispravno unet broj poslovnice. Uneseni podaci se proveravaju u odnosu na regex formu “2\d\d\d00” (da pocniju sa “2” pa idu 3 cifre I zavrsava se sa “00”). Ako nije ispunjen ovaj uslov izlazi poruka “Pogresno unet broj poslovnice” I focus se stavlja na polje za unos broja poslovnice. Ako je uslov ispunjen program nastavlja dalje.
Porvera da li je izabrani datum NULL (nije izabran) ili da li je izabrani datum >= danasnjeg datuma. Ako je bilo sta od ta dva, izlazi poruka “Morate da izaberete datum I da bude pre danasnjeg datuma” I focus se stavlja na DatePicker. Ako nije jedna od ove dve situacije program nastavlja dalje.
Provera da li je datum vise od 7 dana unazad ( tacan kod glasi: SelectedDate < now.AddDays(-8) ). Ako je izabran datum vise od 7 dana unazad, izlazi poruka “Izabrani datum ne sme da bude stariji od 7 dana” I focus se stavlja na DatePicker. Ukoliko je uslov kako treba program nastavlja dalje.
Provera da li je ispravno unet broj blagajne. Prvo proveravamo opet da li je dobar unos broja poslovnice. Ako nije izlazi poruka “Poslovnica nije dobro uneta”. Ako jeste, od broja poslovnice uzimamo 3 identifikaciona broja poslovnice. Proveravamo da li je unesen prazan string u polje broja blagajne. Ako nije proveravamo da li uneseni podaci odgovaraju regex formi “[1-9]\d\d\d” (prvi broj da bude od 1 do 9 I onda 3 cifre) I da li poslednja 3 broja blagajne su ista kao I 3 identifikaciona broja poslovnice. Ako ovi uslovi nisu ispunjeni izlazi poruka “Blagajna nije uneta kako treba” I focus se stavlja na polje za unos blagajne. Ako je sve kako treba program nastavlja dalje
Da bi program nastavio mora opet da ispuni uslove: a. Tacno unet broj poslovnice (tacka 1) b. Izabrani datum nije NULL c. Izabrani datum je pre danasnjeg d. Izabrani datum nije pre vise od 7 dana (u kodu 8 da bi obuhvatio I sedmi dan) e. Tacno unet broj blagajne (tacka 4) Ako su svi uslovi ispunjeni program nastavlja dalje
Proveravamo da li se ping-uje adresa “192.168.1.46”. Ako ping-ovanje nije uspelo izlazi poruka “Ne pingujem server 192.168.1.46”. Ako je uspesno ping-ovan server, program nastavlja dalje
Provera da li uneta poslovnica postoji u bazi. Ako preko upita “select POSL FROM BI_POSL WHERE POSL = {broj poslovnice} and AKTIVNA = 'D'” dobijemo podatak (razlicit od null) program smatra da poslovnica postoji u bazi I nastavlja dalje. Ukoliko uslov nije ispunjen izlazi poruka “Poslovnica ne postoji u bazi” I focus se stavlja na polje za unos broja poslovnice.
Provera da uneta blagajna postoji u bazi za unetu poslovnicu. Ako preko upita “select BLAG_POSL from BI_BLAGPOSL where BLAG_POSL = {broj blagajne} and MATICNA_POSL = {broj poslovnice} and AKTIVNA = 'D'” dobijemo podatak (razlicit od null) program smatra da blagajna u toj poslovnici postoji u bazi I nastavlja dalje. Ukoliko uslov nije ispunjen izlazi poruka “Blagajna ne postoji u bazi ili u toj poslovnici” I focus se stavlja na polje za unos blagajne.
Odabrani datum convertujemo u string u formatu “yyyyMMdd”
Pokusavamo da dobijemo IP adresu kase. Za to koristimo upit “select IP_LOKALNI_NASLOV, blag_posl from BI_BLAGPOSL where MATICNA_POSL = {Broj poslovnice} and blag_posl = {Broj Blagajne}”. Ako ne uspemo da procitamo podatak iz baze izlazi poruka “NO DATA FOUND”. Ako ne uspemo da dobijemo IP adresu blagajne, ista se postavlja na vrednost NULL.
Provera da li je IP adresa kase jednaka NULL. Ako jeste izlazi poruka “IP BLAG JE NULL”. Ako nije, program nastavlja dalje
Pokusavamo da ping-ujemo ip kase. Ukoliko ne uspemo, Izlazi poruka “Ne pingujem kasu!” I Label statusa se menja na “Ne pingujem kasu!” (font postaje bold, povecava se za 5px, I boji se u crvenu boju). Ako uspesno pingujemo kasu, program nastavlja dalje.
Provera da li su “Odhodi” dobri. Uzimamo IP adresu kase I pravimo putanju \IPKASE\RCL\xmlblag\odhodi. Na toj putanji proveravamo koliko ima file-ova. Ako ima 3 ili vise file-ova Label statusa se menja da pise “Odhodi nisu prazni, resetuj sepis” (font se boldira, povecava za 5px I boji u crveno). Ako imaju 2 ili manje program nastavlja dalje.
Label status se menja na “Odhodi su OK 😊”
DataGrid se popunjava podacima iz baze, dobijenim upitom “select ODPRTA, TIP_ODPIS, BLAG_POSL, KODA from Bip_prom where KODA in ('BK','BZ') and POSL = {Broj Poslovnice} and DATUM = '{Datum}' order by BLAG_POSL DESC”
Provera da li je blagajna vec zatvorena. Pozivamo podatke iz baze uz upit “select ODPRTA, TIP_ODPIS, BLAG_POSL from Bip_prom where KODA in ('BK','BZ') and POSL = {Broj Poslovnice} and DATUM = '{Datum}' order by BLAG_POSL DESC”. Provera se vrsi tako sto prolazimo kroz redove gde je BLAG_POSL jednak unetom broju blagajne, proveravamo da li je “ODPRTA” jednaka “” I TIP_ODPIS” jednak “4”. Ako jesu izlazi poruka “Blagajna je vec zatvorena”. Ako negde nije “ODPRTA” jednaka “” I TIP_ODPIS” jednak “4” program nastavlja dalje.
Property Visibility, na dugmetu “Zatvori”, se menja sa “Hidden” na “Visible”. Cime user interface dobija jos jedno dugme “Zatvori !”.
Ako dugme proveri dodje do tacke 17, program ce izgledati ovako:

Pritiskom na dugme “Zatvori !” pokrece se (tacke 2-17 su iste kao prvih 16 za dugme “Provera”):

Izlazi prozor sa porukom “Da li ste sigurni da zelite da zatvorite blagajnu?”. Dugmici na prozoru su “Yes”, “No”, “Cancel”. Pritiskanjem na dugmice “No”, “Cancel” ili izlazkom iz ovog prozora izlazi poruka “Prekinut process zatvaranja blagajne” I property Visibility, na dugmetu “Zatvori”, se menja sa “Visible” na “Hidden”. Cime se user interface vraca na pocetno stanje. Ako se pritisne dugme “Yes” program nastavlja dalje.
Provera da li je ispravno unet broj poslovnice. Uneseni podaci se proveravaju u odnosu na regex formu “2\d\d\d00” (da pocniju sa “2” pa idu 3 cifre I zavrsava se sa “00”). Ako nije ispunjen ovaj uslov izlazi poruka “Pogresno unet broj poslovnice” I focus se stavlja na polje za unos broja poslovnice. Ako je uslov ispunjen program nastavlja dalje.
Porvera da li je izabrani datum NULL (nije izabran) ili da li je izabrani datum >= danasnjeg datuma. Ako je bilo sta od ta dva, izlazi poruka “Morate da izaberete datum I da bude pre danasnjeg datuma” I focus se stavlja na DatePicker. Ako nije jedna od ove dve situacije program nastavlja dalje.
Provera da li je datum vise od 7 dana unazad ( tacan kod glasi: SelectedDate < now.AddDays(-8) ). Ako je izabran datum vise od 7 dana unazad, izlazi poruka “Izabrani datum ne sme da bude stariji od 7 dana” I focus se stavlja na DatePicker. Ukoliko je uslov kako treba program nastavlja dalje.
Provera da li je ispravno unet broj blagajne. Prvo proveravamo opet da li je dobar unos broja poslovnice. Ako nije izlazi poruka “Poslovnica nije dobro uneta”. Ako jeste od broja poslovnice uzimamo 3 identifikaciona broja poslovnice. Proveravamo da li je unesen prazan string u polje broja blagajne. Ako nije proveravamo da li uneseni podaci odgovaraju regex formi “[1-9]\d\d\d” (prvi broj da bude od 1 do 9 I onda 3 cifre) I da li poslednja 3 broja blagajne su ista kao I 3 identifikaciona broja poslovnice. Ako ovi uslovi nisu ispunjeni izlazi poruka “Blagajna nije uneta kako treba” I focus se stavlja na polje za unos blagajne. Ako je sve kako treba program nastavlja dalje
Da bi program nastavio mora opet da ispuni uslove: a. Tacno unet broj poslovnice (tacka 1) b. Izabrani datum nije NULL c. Izabrani datum je pre danasnjeg d. Izabrani datum nije pre vise od 7 dana (u kodu 8 da bi obuhvatio I sedmi dan) e. Tacno unet broj blagajne (tacka 4) Ako su svi uslovi ispunjeni program nastavlja dalje
Proveravamo da li se ping-uje adresa “192.168.1.46”. Ako ping-ovanje nije uspelo izlazi poruka “Ne pingujem server 192.168.1.46”. Ako je uspesno ping-ovan server, program nastavlja dalje
Provera da li uneta poslovnica postoji u bazi. Ako preko upita “select POSL FROM BI_POSL WHERE POSL = {broj poslovnice} and AKTIVNA = 'D'” dobijemo podatak (razlicit od null) program smatra da poslovnica postoji u bazi I nastavlja dalje. Ukoliko uslov nije ispunjen izlazi poruka “Poslovnica ne postoji u bazi” I focus se stavlja na polje za unos broja poslovnice.
Provera da uneta blagajna postoji u bazi za unetu poslovnicu. Ako preko upita “select BLAG_POSL from BI_BLAGPOSL where BLAG_POSL = {broj blagajne} and MATICNA_POSL = {broj poslovnice} and AKTIVNA = 'D'” dobijemo podatak (razlicit od null) program smatra da blagajna u toj poslovnici postoji u bazi I nastavlja dalje. Ukoliko uslov nije ispunjen izlazi poruka “Blagajna ne postoji u bazi ili u toj poslovnici” I focus se stavlja na polje za unos blagajne.
Odabrani datum convertujemo u string u formatu “yyyyMMdd”
Pokusavamo da dobijemo IP adresu kase. Za to koristimo upit “select IP_LOKALNI_NASLOV, blag_posl from BI_BLAGPOSL where MATICNA_POSL = {Broj Poslovnice} and blag_posl = {Broj Blagajne}”. Ako ne uspemo da procitamo podatak iz baze izlazi poruka “NO DATA FOUND”. Ako ne uspemo da dobijemo IP adresu blagajne, ista se postavlja na vrednost NULL.
Provera da li je IP adresa kase jednaka NULL. Ako jeste izlazi poruka “IP BLAG JE NULL”. Ako nije, program nastavlja dalje
Pokusavamo da ping-ujemo ip kase. Ukoliko ne uspemo, Izlazi poruka “Ne pingujem kasu!” I Label statusa se menja na “Ne pingujem kasu!” (font postaje bold, povecava se za 5px, I boji se u crvenu boju). Ako uspesno pingujemo kasu, program nastavlja dalje.
Provera da li su “Odhodi” dobri. Uzimamo IP adresu kase I pravimo putanju \IPKASE\RCL\xmlblag\odhodi. Na toj putanji proveravamo koliko ima file-ova. Ako ima 3 ili vise file-ova Label statusa se menja da pise “Odhodi nisu prazni, resetuj sepis” (font se boldira, povecava za 5px I boji u crveno). Ako imaju 2 ili manje program nastavlja dalje.
Label status se menja na “Odhodi su OK 😊”
DataGrid se popunjava podacima iz baze, dobijenim upitom “select ODPRTA, TIP_ODPIS, BLAG_POSL, KODA from Bip_prom where KODA in ('BK','BZ') and POSL = {Broj Poslovnice} and DATUM = '{Datum}' order by BLAG_POSL DESC”
Provera da li je blagajna vec zatvorena. Pozivamo podatke iz baze uz upit “select ODPRTA, TIP_ODPIS, BLAG_POSL from Bip_prom where KODA in ('BK','BZ') and POSL = {Broj Poslovnice} and DATUM = '{Datum}' order by BLAG_POSL DESC”. Provera se vrsi tako sto prolazimo kroz redove gde je BLAG_POSL jednak unetom broju blagajne, proveravamo da li je “ODPRTA” jednaka “” I TIP_ODPIS” jednak “4”. Ako jesu izlazi poruka “Blagajna je vec zatvorena”. Ako negde nije “ODPRTA” jednaka “” I TIP_ODPIS” jednak “4” program nastavlja dalje.
Pokrece se upit “update BIP_PROM set ODPRTA = '*', TIP_ODPIS = '4' where KODA in ('BK','BZ') and POSL = {Broj Poslovnice} and DATUM = '{Datum}' and BLAG_POSL = {Broj Blagajne}”. Ovim upitom “zatvaramo blagajnu”. Takodje u ovom koraku ipisujemo poruku “Promenjeno je {Broj promena} redova” (odnoseci se na koji broj redova je promenjen u bazi)
Provera da li je blagajna zatvorena, uz pomoc upita “select ODPRTA, TIP_ODPIS, BLAG_POSL from Bip_prom where KODA in ('BK','BZ') and POSL = {Broj Poslovnice} and DATUM = '{Datum}' order by BLAG_POSL DESC”. Ako je zatvorena ispisujemo poruku “Zatvorena je blagajna {Broj Blagajne} u poslovnici {Broj Poslovnice} za datum {Datum}”
DataGrid se popunjava podacima iz baze, dobijenim upitom “select ODPRTA, TIP_ODPIS, BLAG_POSL, KODA from Bip_prom where KODA in ('BK','BZ') and POSL = {Broj Poslovnice} and DATUM = '{Datum}' order by BLAG_POSL DESC”
Label statusa menjamo na “Blagajna je zatvorena”
Property Visibility, na dugmetu “Zatvori”, se menja sa “Visible” na “Hidden”. Cime se user interface vraca na pocetno stanje.
