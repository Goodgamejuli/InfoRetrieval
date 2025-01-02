import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TopNavComponent } from "../../components/top-nav/top-nav.component";
import { SongCardComponent } from "../../components/song-card/song-card.component";

@Component({
  selector: 'app-home',
  imports: [TopNavComponent, SongCardComponent, CommonModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit{

public songCards= [
    {
      thumbnail: 'assets/song_placeholder.png',
      title: 'Test song',
      description: 'Test einer song description oder so',
    },
    {
      thumbnail: 'assets/song_placeholder.png',
      title: 'd song',
      description: 'Test einer song description oder so',
    },
    {
      thumbnail: 'assets/song_placeholder.png',
      title: 's song',
      description: 'Test einer song description oder so',
    },
    {
      thumbnail: 'assets/song_placeholder.png',
      title: 'Test song',
      description: 'Test einer song description oder so',
    },
    {
      thumbnail: 'assets/song_placeholder.png',
      title: 'd song',
      description: 'Test einer song description oder so',
    },
    {
      thumbnail: 'assets/song_placeholder.png',
      title: 's song',
      description: 'Test einer song description oder so',
    },{
      thumbnail: 'assets/song_placeholder.png',
      title: 'Test song',
      description: 'Test einer song description oder so',
    },
    {
      thumbnail: 'assets/song_placeholder.png',
      title: 'd song',
      description: 'Test einer song description oder so',
    },
    {
      thumbnail: 'assets/song_placeholder.png',
      title: 's song',
      description: 'Test einer song description oder so',
    },
  ]

  ngOnInit(): void {
    
  }
}
