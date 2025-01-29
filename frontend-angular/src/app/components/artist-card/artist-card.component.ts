import { Component, inject, Input } from '@angular/core';
import { Router } from '@angular/router';
import { SongDataService } from '../../services/song-data.service';
import { SongDTO } from '../../models/songDto';
import { OpenSearchService } from '../../services/opensearch.service';

@Component({
  selector: 'app-artist-card',
  imports: [],
  templateUrl: './artist-card.component.html',
  styleUrl: './artist-card.component.css'
})
export class ArtistCardComponent {
  opensearchService = inject(OpenSearchService);
  
  @Input() public artist!: string;
  @Input() public genre!: string;

  @Input() public coverUrl!: string;

  constructor() { }

  findSongsOfArtist() {
    this.opensearchService.searchForSongsOfArtist(this.artist, null, 0);
  }

}
