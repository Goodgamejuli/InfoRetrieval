import { HttpClient, HttpParams } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { environment } from "../../environments/environment.development";
import { map, Observable } from "rxjs";
import { SongDTO } from "../models/songDto";
import { ArtistDto } from "../models/artistDto";
import { AlbumDto } from "../models/albumDto";

@Injectable({
    providedIn: 'root',
})

export class OpenSearchService {
    private http = inject(HttpClient);
    private openSearchApiUrl = environment.apiURL + '/api/OpenSearch';

    public minScoreThreshold: number = 0.10;

    public songs: SongDTO[] = [];
    public artists: ArtistDto[] = [];
    public albums: AlbumDto[] = [];

    public get(): Observable<any> {
        return this.http.get(this.openSearchApiUrl);
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

        var results = this.http.post<SongDTO>(`${this.openSearchApiUrl}/CrawlAllSongsOfArtist`,{}, {params})
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
        genreSearch: string | null = null,
        dateSearch: string | null = null,
        hitCount: number = 10
    ){
        if(searchValue == null || searchValue == "") {
            this.songs = [];
            return;
        }

        // Define URL-Parameters
        let params = new HttpParams()
            .set('search', searchValue)
            .set('query', query)
            .set('hitCount', hitCount.toString())
            .set('minScoreThreshold', this.minScoreThreshold);
        
            if(genreSearch) 
                params = params.set('genreSearch', genreSearch)

            if(dateSearch)
                params = params.set('dateSearch', dateSearch)

        var results = this.http.get<SongDTO[]>(`${this.openSearchApiUrl}/FindSongs`, {params})
            .subscribe({
                next: (songs) => {
                    if(songs.length == 0)
                        console.log("kein song gefunden!");

                    this.songs = songs;
                },
                error: (err) => {
                    console.error('Error findingsongs: ', err);
                }
            });
    }

//////////////////////////////// Artist Search Stuff /////////////////////////////

    public searchForArtist(
        searchValue: string,
        maxHitCount: number = 10
    ) {
        //Define params
        const params = new HttpParams()
        .set('search', searchValue)
        .set('maxHitCount', maxHitCount)

        this.http.get<ArtistDto[]>(`${this.openSearchApiUrl}/FindArtists`, {params})
            .subscribe( {
                next: (artists) => {
                    if(artists.length == 0)
                        console.log("Kein artist gefunden!");
                    this.artists = artists;
                    console.log(this.artists);
                },
                error: (err) => {
                    console.error('Error finding artists: ', err);
                }
            });
    }

    public searchForSongsOfArtist(
        artist: string,
        search: string | null,
        minScoreThreshold: number
    ) {
        //Define params

        let params = new HttpParams()
            .set('artist', artist)
            .set('minScoreThreshold', minScoreThreshold)
        
        if(search) 
            params = params.set('search', search)

        this.http.get<SongDTO[]>(`${this.openSearchApiUrl}/FindSongsOfArtist`, {params})
            .subscribe( {
                next: (songs) => {
                    if(songs.length == 0)
                        console.log("Kein passender Song gefunden!");
                    this.songs = songs;
                    console.log(this.songs);
                },
                error(err) {
                    console.error('Error finding songs in album: ', err);
                },
            })
    }


//////////////////////////////// Album Search Stuff /////////////////////////////
    public searchForAlbums(
        searchValue: string,
        maxHitCount: number = 10
    ) {
        //Define params
        const params = new HttpParams()
            .set('search', searchValue)
            .set('maxHitCount', maxHitCount)

        this.http.get<AlbumDto[]>(`${this.openSearchApiUrl}/FindAlbums`, {params})
            .subscribe( {
                next: (albums) => {
                    if(albums.length == 0)
                        console.log("Kein album gefunden!");
                    this.albums = albums;
                    console.log(this.artists);
                },
                error: (err) => {
                    console.error('Error finding album: ', err);
                }
            });
    }

    public searchForSongsInAlbum(
        albumTitle: string,
        search: string | null,
        minScoreThreshold: number
    ) {
        //Define params

        let params = new HttpParams()
            .set('albumTitle', albumTitle)
            .set('minScoreThreshold', minScoreThreshold)

        if(search) 
            params = params.set('search', search)

        this.http.get<SongDTO[]>(`${this.openSearchApiUrl}/FindSongsInAlbum`, {params})
            .subscribe( {
                next: (songs) => {
                    if(songs.length == 0)
                        console.log("Kein passender Song gefunden!");
                    this.songs = songs;
                    console.log(this.songs);
                },
                error(err) {
                    console.error('Error finding songs in album: ', err);
                },
            })
    }



    public async checkIfOpenSearchIsReachable(): Promise<boolean> {
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

    public async checkIfBackendIsReachable(): Promise<boolean> {
        try {
            const response = await this.http
                .get(`https://localhost:7238/api/Database/IsReachable`,  { responseType: 'text' })
                .toPromise();
            console.log("Backend ist erreichbar.");
            return true;
        } catch (error) {
            console.error("Backend nicht erreichbar.", error);
            return false;
        }
    }


}