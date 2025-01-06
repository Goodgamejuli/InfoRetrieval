import { HttpClient } from "@angular/common/http";
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

    public get(): Observable<any> {
        return this.http.get(this.apiURL);
    }

    public getSongTest() {
        var songDto: SongDTO = {
            album: "",
            artist: "",
            title: "",
            lyrics: "",
            release: "",
            genre: []
        }

        this.http.get<SongDTO>(this.apiURL)
        .subscribe(data =>  {
            songDto.title = data.title;
            songDto.album = data.album;
        });
        console.log(songDto);
    }
}