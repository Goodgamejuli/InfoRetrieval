import { Component, NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-admin',
  imports: [FormsModule],
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.css'
})
export class AdminComponent {
  artistToFetch: string = '';
  useSpotifyApi: boolean = true;
  useMusicBrainzApi: boolean = true;

  fetchSongsOfArtist() {
    if(this.artistToFetch == '' 
      || (!this.useSpotifyApi && !this.useMusicBrainzApi)) {
        alert("Artist muss eingegeben werden und mindestens eine API muss zum suchen genutzt werden!")
        return;
    }

    console.log('Artist Name:', this.artistToFetch);
    console.log('Use Spotify API:', this.useSpotifyApi);
    console.log('Use Spotify MusicBrainzAPI:', this.useMusicBrainzApi);
  }
}
