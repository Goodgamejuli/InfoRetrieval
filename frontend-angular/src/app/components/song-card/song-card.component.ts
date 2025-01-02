import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';
import { SongDataService } from '../../services/song-data.service';

@Component({
  selector: 'app-song-card',
  imports: [],
  templateUrl: './song-card.component.html',
  styleUrl: './song-card.component.css'
})
export class SongCardComponent {
  @Input() public playlistThumbnail!: string;
  @Input() public title!: string;
  @Input() public description!: string;
  @Input() public id!: string | number;
  @Input() public link!: string;

  /**
   *
   */
  constructor(private router: Router, private songDataService: SongDataService) { }

  onNavigateToSong() {
    this.songDataService.songData.next({
      
    });

    this.router.navigateByUrl(`/song/${this.id}`, {
      state: {
      thumbnail: this.playlistThumbnail,
      title: this.title,
      description: this.description,
      link: this.link,
      id: this.id
      },
    });
  }

}
