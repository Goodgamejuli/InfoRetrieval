# README.md

## Information MAangement und Data Retrieval 

### Beschreibung

Dieses Projekt ist eine ASP.NET Core 9 Web API, die mit einem Angular-Frontend interagiert. Die Backend-Anwendung verwendet OpenSearch als Such- und Analyse-Engine und wird mit Docker betrieben. Die gesamte Entwicklung erfolgt in Visual Studio.

## Voraussetzungen

### Allgemeine Anforderungen

- Visual Studio 2022 (mit .NET 9-Unterstützung)
- Node.js (für Angular, empfohlen: neueste LTS-Version)
- Angular CLI
- Docker (zum Starten von OpenSearch)
- Spotify Account

### Installierte Abhängigkeiten

Stelle sicher, dass folgende Programme installiert sind:

- .NET 9 SDK: [Download hier](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- Node.js und npm: [Download hier](https://nodejs.org/)
- Angular CLI: Installierbar über npm mit:
  ```sh
  npm install -g @angular/cli
  ```
- Docker: [Download hier](https://www.docker.com/get-started/)

## Setup und Installation

### 1. OpenSearch mit Docker starten

Starten Sie Open Search mit Docker

Falls Sie noch kein OpenSearech-Container in Docker haben, können Sie unter `https://opensearch.org/docs/latest/install-and-configure/install-opensearch/docker/` die Schriite nachverfolgen, um einen solchen Container anzulegen. Die benötigte docker-compose.yml befindet sich bereits im Projekt. Diese können Sie nutzen oder eine neue anlegen.


### 2. Backend (ASP.NET Core 9) starten

1. Öffne das Projekt in **Visual Studio 2022**.
2. Stelle sicher, dass das .NET 9 SDK installiert ist.
3. Starten Sie das Projekt in Visual Studio. Beachten Sie, dass https eingestellt sein muss (sieh Bild)
   ![image](https://github.com/user-attachments/assets/a714a895-f174-40b0-bfc7-155321f45401)
Das Backend läuft nun unter `https://localhost:7238`. Falls sich nicht automatisch Swagger startet können Sie dies über `https://localhost:7238/swagger/index.html` erreichen.

### 3. Frontend (Angular) starten

1. Navigiere in den Angular-Projektordner
2. Dies können Sie jetzt mit Vs Code öffnen, müssen es aber nicht. Sie können auch einfach die Kommandozeile aus diesem Ordner heraus öffenen.
3. Geben Sie in der Kommandozeile diesen Befehl an, um das Frontend local zu starten:
   ```sh
   ng serve -o
   ```
4. Es sollte sich automatisch das Frontend in Ihrem Browser öffen. Falls nicht, ist das Frontend unter `http://localhost:4200` erreichbar.
5. Sie können sich im Frontend nun anzeigen lassen, ob OpenSearch und das C#-Backend erreichbar sind. Dazu müssen Sie auf die AdminSeite im Frontend klicken und können über "Erreichabrkeit des Backends prüfen" testen, ob das Backend erreichbar ist.

## Wichtig

### Datenbank und OpenSearch aufsetzen
Damit das Projekt laufen kann, muss sowohl ein Index in OpenSearch als auch eine Datenbankstruktur angelegt werden. Dies muss initial einmalig passieren oder kann auch immer wieder vorgenommen werden, wenn sie die Datenbank und OpenSearch neu aufsetzen wollen.
**Wichtig! Es müssen immer beide Schritte ausgeführt werden. Sonst kommt es zu Problemen**

1. Datenbank neu aufsetzen
   Führen sie in Swagger diese Methode aus:
   ![image](https://github.com/user-attachments/assets/b3f81ff5-ea38-4121-a4ae-5729466de825)
   Die einstellbaren Werte sollen dem default value entsprechen.

   Sie können auch im Frontend auf der AdminSeite den Button "Datenbank neu aufsetzen" klicken!

2. OpenSearch neu aufsetzen
   Führen sie in Swagger diese Methode aus:
   ![image](https://github.com/user-attachments/assets/b7f3b707-2c81-44a3-b3bc-d3c77fa36be4)

   Sie können auch im Frontend auf der AdminSeite den Button "OpenSearch neu aufsetzen" klicken!

### Verbindung zu Spotify
Um das Projekt testen zu können, müssen Sie in dem genutzen Browser mit Ihrem Spotify account eingeloggt sein. Zudem müssen Sie als Entwickler im der Spotify genutzen SpotifyAPI hinzugefügt werden. Dafür benötige ich Ihre Spotify-Email und Nutzername. 

Falls Sie das nicht Preisgeben wollen, können Sie sich auch selbst eine SptoifyAPI-App anlegen.
Einfach hier durch die Dokumentation lesen und eine App anlegen `https://developer.spotify.com`. 
Wenn Sie das getan haben, müssen Sie noch die AppDaten im Backend mit Ihren neuen Daten aktualiesieren. Dafür gehen Sie in die SpotifyAPIService.cs-Klasse und geben für die Variable ClientID Ihre neue ClientId und für die Variable ClientSecret Ihr neues ClientSecret an.

## Lizenz

Dieses Projekt steht unter der MIT-Lizenz.

