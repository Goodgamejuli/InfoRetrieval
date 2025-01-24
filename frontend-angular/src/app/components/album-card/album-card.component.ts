import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';
import { SongDataService } from '../../services/song-data.service';
import { SongDTO } from '../../models/songDto';

@Component({
  selector: 'app-album-card',
  imports: [],
  templateUrl: './album-card.component.html',
  styleUrl: './album-card.component.css'
})
export class AlbumCardComponent {
  @Input() public title!: string;
  @Input() public release!: string;
  @Input() public artist!: string;


  @Input() public coverUrl!: string;

  constructor(private router: Router, private songDataService: SongDataService) 
  {
    //TODO: delete and set via api-call
    this.coverUrl = "https://i.scdn.co/image/ab67616d00001e02ff9ca10b55ce82ae553c8228";
  }

}
