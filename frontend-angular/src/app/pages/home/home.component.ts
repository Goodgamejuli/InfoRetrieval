import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TopNavComponent } from "../../components/top-nav/top-nav.component";
import { SongCardComponent } from "../../components/song-card/song-card.component";
import { SearchBarService } from '../../services/searchbar.service';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-home',
  imports: [TopNavComponent, SongCardComponent, CommonModule, RouterModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit{

public songCards= [
    {
      song_id: 1,
      thumbnail: 'assets/song_placeholder.png',
      title: 'Test song',
      description: 'Test einer song description oder so',
      song_link: 'assets/songs/test_song.mp3'
    },
    {
      song_id: 1,
      thumbnail: 'assets/song_placeholder.png',
      title: 'Test song',
      description: 'Test einer song description oder so',
      song_link: ''
    }
  ];

  constructor(private searchBarService: SearchBarService) {  }

  ngOnInit(): void {  }

  onNavigate(pageName: string) {
    if(pageName === 'search')
      this.searchBarService.isSearchVisible.next(true);
    else
    this.searchBarService.isSearchVisible.next(false);
  }

}
