# MockApp

To repozytorium zawiera prototyp aplikacji do mockowania odpowiedzi, w celu dewelopmentu aplikacji frontendowej.

## Zawartość repozytorium

`/mockapp-frontend` - Aplikacja frontendowa Angular + Angular Material
`/MockApi` - API w ASP.NET Core z Entity Framework Core + Postgres

## Uruchomienie projektu

### Backend

1. Z katalogu `MockApi` uruchom solucję `MockApi.sln` w Visual Studio 2022.
2. Skonfiguruj połączenie z bazą danych w `appsettings.json`
3. W terminalu po przejściu do katalogu `MockApi/MockApi/` wykonaj migrację:
```bash
dotnet ef database update
```
4. Uruchom aplikację. Domyślny adres API: `https://localhost:44313`

### ngrok

W celu debugowania usługi Stripe należy skorzystać z ngrok:
1. W terminalu wpisz
```bash
ngrok http https://localhost:44313
```
2. Dostaniesz link (np. `https://9a3b-123-45-67-89.eu.ngrok.io`).
3. Podajesz ten link do Stripe jako URL webhooka: `https://9a3b-123-45-67-89.eu.ngrok.io/api/stripe/webhook`
4. Wtedy Stripe przy zmianie subskrypcji uderzy do metody `Webhook` w `StripeController`.

### Frontend

1. W katalogu `mockapp-frontend` zainstaluj zależności
```bash
npm install
```
2. Uruchom aplikację. Domyślny adres to: `http://localhost:4200`
```bash
npm start
```