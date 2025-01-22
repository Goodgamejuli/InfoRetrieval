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
  @Input() public playlistThumbnail!: string;
  @Input() public title!: string;
  @Input() public description!: string;
  @Input() public id!: string | number;
  @Input() public link!: string;

  @Input() public song!: SongDTO;

  @Input() public coverUrl!: string;

  constructor(private router: Router, private songDataService: SongDataService) 
  {
    //TODO: delete and set via api-call
    this.coverUrl = "https://i.scdn.co/image/ab67616d00001e02ff9ca10b55ce82ae553c8228";
  }

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
