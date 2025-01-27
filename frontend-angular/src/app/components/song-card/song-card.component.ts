import { Component, inject, Input } from '@angular/core';
import { Router } from '@angular/router';
import { SongDataService } from '../../services/song-data.service';
import { SongDTO } from '../../models/songDto';
import { PlaybarService } from '../../services/playbar.service';

@Component({
  selector: 'app-song-card',
  imports: [],
  templateUrl: './song-card.component.html',
  styleUrl: './song-card.component.css'
})
export class SongCardComponent {
  playbarService = inject(PlaybarService);

  @Input() public title!: string;
  @Input() public lyrics!: string;
  @Input() public id!: string;
  @Input() public album!: string;
  @Input() public release!: string;
  @Input() public artist!: string;
  @Input() public genre!: string[];

  @Input() public song!: SongDTO;

  @Input() public coverUrl!: string;
  @Input() public spotifyPlayUrl!: string;


  constructor(private router: Router, private songDataService: SongDataService)  {
    this.coverUrl = "https://i.scdn.co/image/ab67616d00001e02ff9ca10b55ce82ae553c8228";
   }

  playThisSong() {
    this.playbarService.playSong(this.spotifyPlayUrl);
  }

  /*onNavigateToSong() {
    this.songDataService.songData.next({
      
    });

    this.router.navigateByUrl(`/song/${this.id}`, {
      state: {
      thumbnail: this.cover,
      title: this.title,
      description: this.description,
      id: this.id
      },
    });
  }*/

}
