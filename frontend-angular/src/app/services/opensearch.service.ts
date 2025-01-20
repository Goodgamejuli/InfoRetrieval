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
}