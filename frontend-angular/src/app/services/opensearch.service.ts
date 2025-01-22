import { HttpClient, HttpParams } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { environment } from "../../environments/environment.development";
import { map, Observable } from "rxjs";
import { SongDTO } from "../models/songDto";

@Injectable({
    providedIn: 'root',
})

export class OpenSearchService {
    private http = inject(HttpClient);
    private apiURL = environment.apiURL + '/api/OpenSearch';

    public Songs: SongDTO[] = [];

    public get(): Observable<any> {
        return this.http.get(this.apiURL);
    }


    public crawlAllSongsOfArtist(
        artist: string,
        useSpotifyApi: boolean,
        useMusicBrainzApi: boolean) {

        // Define URL-Parameters
        const params = new HttpParams()
        .set('artistName', artist)
        .set('useSpotifyApi', useSpotifyApi)
        .set('useMusicBrainzApi', useMusicBrainzApi);

        console.log(params.toString());

        var results = this.http.post<SongDTO>(`${this.apiURL}/CrawlAllSongsOfArtist`,{}, {params})
            .subscribe({
                next: () => {
                    console.log("songs crawled")
                },
                error: (err) => {
                    console.error('Error crawling songs: ', err);
                }
            });
    }

    public searchForSongs(
        searchValue: string,
        query: string = 'title;album;artist;lyrics',
        hitCount: number = 10
    ){
        if(searchValue == null || searchValue == "") {
            this.Songs = [];
            return;
        }

        // Define URL-Parameters
        const params = new HttpParams()
        .set('search', searchValue)
        .set('query', query)
        .set('hitCount', hitCount.toString());

        console.log(params.toString());

        var results = this.http.get<SongDTO[]>(`${this.apiURL}/FindSongs`, {params})
            .subscribe({
                next: (songs) => {
                    if(songs.length == 0)
                        console.log("kein song gefunden!")

                    this.Songs = songs;
                },
                error: (err) => {
                    console.error('Error findingsongs: ', err);
                }
            });
    }



    public async checkIfBackendIsReachable(): Promise<boolean> {
        try {
            const response = await fetch('http://localhost:9200', { 
                method: 'GET',
                mode: 'no-cors' 
            });
            console.log("Erreichbar");
            return true; // Erfolgreich
        } catch (error) {
            console.log("Nicht erreichbar");
            return false; // Fehlgeschlagen
        }
    }


}