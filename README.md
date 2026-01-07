## Litacka - popis projektu

Tento projekt je Proof of Concept (PoC) aplikace postavená na ASP.NET Core (nejnovější .NET), která demonstruje:

- autentizaci a autorizaci pomocí ASP.NET Core Identity (cookies)
    
- práci s rolemi (super-admin, admin, manager, user, new-user)
    
- jednoduchý business model „Card“ (lítačka) přiřazený k uživateli
    
- administraci uživatelů a karet přes Razor Pages
    
- jednoduché API endpointy pro čtení stavu a platnosti karty
    
- OpenAPI dokumentaci pomocí Scalar
    
- oddělení logiky do services a návratové typy Result místo vyhazování výjimek
    

Projekt slouží jako demonstrační ukázka architektury a není zamýšlen jako produkční řešení.

---

## Požadavky

- Visual Studio 2026
    
- .NET SDK odpovídající projektu (instaluje se spolu s VS 2026)
        

Poznámka: Projekt cílí na nejnovější .NET. Ve starších verzích Visual Studia nebo .NET nemusí jít spustit.

---

## Jak projekt spustit

1. Naklonuj repozitář z Git repozitáře.
    
2. Otevři solution ve Visual Studio 2026.
    
3. Nastav startup project na LitackaApi.
    
4. Spusť aplikaci (F5 nebo Ctrl+F5).
    

Aplikace poběží na lokální adrese dle nastavení launch profilu projektu.

---

## Databáze

Projekt používá SQLite databázi.

- Databázový soubor je součástí repozitáře
    
- Databáze již obsahuje:
    
    - Identity tabulky (uživatelé, role)
        
    - Business tabulky Cards a CardStatuses
        
- Není nutné vytvářet migrace ani seedovat data ručně
    

Databázi je možné otevřít např. v nástroji DB Browser for SQLite.

---

## Přihlášení a role

Aplikace používá ASP.NET Core Identity s cookies.

### Bootstrap super-admin

V souboru appsettings.json je definován bootstrap účet:

BootstrapAdmin:

- Email: …
    
- Password: …
    

Při startu aplikace se:

- ověří existence rolí
    
- ověří nebo vytvoří super-admin účet
    

---

## Uživatelské rozhraní (Razor Pages)

### Domovská stránka

URL: /

Zobrazuje přihlášenému uživateli:

- stav jeho karty
    
- datum expirace (ve formátu dd.M.yyyy)
    

Pokud uživatel nemá přiřazenou kartu, zobrazí se informační hláška.

---

### Administrace – uživatelé

URL: /Admin/Users

- přístup pouze pro role admin a super-admin
    
- základní správa uživatelských účtů
    

---

### Administrace – karty

URL: /Admin/Cards

- přístup pouze pro role admin a super-admin
    

Funkcionalita:

- každý uživatel může mít maximálně jednu kartu
    
- pokud karta neexistuje:
    
    - tlačítko „Založit kartu“
        
    - výchozí stav je první položka v CardStatuses (Order = 1)
        
    - expirace je nastavena na dnešní datum + 3 roky
        
- pokud karta existuje:
    
    - lze změnit stav karty
        
    - lze změnit datum expirace
        
- karta se nikdy nemaže, „zrušení“ se řeší změnou stavu
    

---

## API endpointy

### Health
GET /health

- vrací "Healthy" pokud aplikace běží
- vytvořena standardním postupem pro tento typ EP konfigurací Project.cs

### Platnost karty

GET /cards/{cardId}/validity

- cardId je hodnota Id z tabulky Cards
    
- response je prostý text
    
- formát data: dd.M.yyyy
    

---

### Stav karty

GET /cards/{cardId}/state

- response je prostý text
    
- hodnota odpovídá CardStatuses.Name
    

---

### Autorizace API

Oba endpointy vyžadují roli:

- manager nebo vyšší (admin, super-admin)
    

Chování:

- nepřihlášený uživatel → 401 Unauthorized
    
- přihlášený bez role → 403 Forbidden
    
- oprávněný uživatel → 200 OK
    

---

## API přihlášení (pro testování)

Pro testování API (např. pomocí .http souborů nebo Postmanu) je k dispozici jednoduchý login endpoint:

POST /api/auth/login

Body požadavku obsahuje:

- email
    
- password
    
- rememberMe (true/false)
    

Po úspěšném přihlášení se nastaví Identity cookie, která se použije pro další API volání.

Odhlášení:  
POST /api/auth/logout

---

## OpenAPI dokumentace

V development režimu je dostupná OpenAPI dokumentace:

- UI: /scalar
    
- OpenAPI JSON: /openapi/v1.json
    

Odkaz na dokumentaci je přidaný také do navigačního menu aplikace.

---

## Testy

- Testy jsou v samostatném projektu založeném na xUnit
    
- Používají SQLite in-memory databázi
    
- Testují services vracející Result
    

Spuštění testů:

- přes Visual Studio (Test Explorer)
    
- nebo z příkazové řádky pomocí příkazu dotnet test
    

---

## Poznámky k PoC

- Číselník stavů (CardStatuses) je považován za stabilní
    
- Pro PoC se neupravuje přes UI, případné změny lze provést přímo v databázi
    
- Projekt klade důraz na:
    
    - jednoduchost
        
    - čitelnost
        
    - ukázku architektonických principů (services, Result pattern, role-based access)
        
