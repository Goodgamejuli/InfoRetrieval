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

    public Songs: SongDTO[] = [];

    public get(): Observable<any> {
        return this.http.get(this.apiURL);
    }

    public getSongTest() {
        var song: SongDTO = {
            album: "test",
            artist: "test",
            title: "TESET",
            lyrics: "sdafadfddf",
            release: "22.02.2333",
            genre: ["metal", "pop"], 
            id: "3234d"
        }
        this.Songs.push(song);
        console.log(song);
    }

    public getSong() {
        var song: SongDTO = {
            album: "",
            artist: "",
            title: "",
            lyrics: "",
            release: "",
            genre: [], 
            id: ""
        }

        this.http.get<SongDTO>(this.apiURL)
        .subscribe(data =>  {
            song.title = data.title;
            song.lyrics = data.lyrics;
            song.album = data.album;
            song.artist = data.artist;
            song.genre = data.genre;
            song.id = data.id;
            song.release = data.release;
        });

        this.Songs.push(song);
        console.log(song);
    }
}