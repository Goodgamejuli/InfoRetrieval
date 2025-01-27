import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class PlaybarService {

  public songToPlayUrl: string | null = null;

  constructor() { }

  playSong(embedUrl: string) {
    this.songToPlayUrl = embedUrl;
    console.log("URL geändert zu: " + this.songToPlayUrl);
  }

  stopSong() {
    this.songToPlayUrl = null;
  }
}
