import { Component, OnInit } from '@angular/core';
import { SongDataService } from '../../services/song-data.service';
import { ActivatedRoute, Router } from '@angular/router';
import { map } from 'rxjs';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-song',
  imports: [CommonModule],
  templateUrl: './song.component.html',
  styleUrl: './song.component.css'
})
export class SongComponent implements OnInit{
  public songData!: any;

  constructor(private songDataService: SongDataService, private activatedRoute: ActivatedRoute, private router: Router) {  }
  
  ngOnInit(): void {
    this.activatedRoute.params.pipe(map((d) => window.history.state)).subscribe(data => this.songData = data);

    if(this.songData?.link === undefined) {
      this.router.navigate(['/']);
      window.alert('No Song available For Selection')
    }
  }


}
