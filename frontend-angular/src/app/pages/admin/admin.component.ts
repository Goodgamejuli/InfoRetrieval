import { Component, inject, NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { OpenSearchService } from '../../services/opensearch.service';

@Component({
  selector: 'app-admin',
  imports: [FormsModule],
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.css'
})
export class AdminComponent {
  opensearchService = inject(OpenSearchService);

  // Checking if backend is available
  isOpenSearchReachable: boolean = false;
  isBackendReachable: boolean = false;

  
  // Fetching songs of artist
  artistToFetch: string = '';
  useSpotifyApi: boolean = true;
  useMusicBrainzApi: boolean = true;

  checkReachabilityOfBackends() {
    this.opensearchService.checkIfBackendIsReachable().then(result => this.isBackendReachable = result);
    this.opensearchService.checkIfOpenSearchIsReachable().then(result => this.isOpenSearchReachable = result);
  }

  fetchSongsOfArtist() {
    if(this.artistToFetch == '' 
      || (!this.useSpotifyApi && !this.useMusicBrainzApi)) {
        alert("Artist muss eingegeben werden und mindestens eine API muss zum suchen genutzt werden!")
        return;
    }

    console.log('Artist Name:', this.artistToFetch);
    console.log('Use Spotify API:', this.useSpotifyApi);
    console.log('Use Spotify MusicBrainzAPI:', this.useMusicBrainzApi);

    this.opensearchService.crawlAllSongsOfArtist(this.artistToFetch, this.useSpotifyApi, this.useMusicBrainzApi);
  }
}
