import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';
import { SongDataService } from '../../services/song-data.service';
import { SongDTO } from '../../models/songDto';

@Component({
  selector: 'app-artist-card',
  imports: [],
  templateUrl: './artist-card.component.html',
  styleUrl: './artist-card.component.css'
})
export class ArtistCardComponent {
  
  @Input() public artist!: string;
  @Input() public genre!: string;

  @Input() public coverUrl!: string;

  constructor() { }

}
