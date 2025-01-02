import { Component, OnInit } from '@angular/core';
import { TopNavComponent } from "../../components/top-nav/top-nav.component";
import { SongCardComponent } from "../../components/song-card/song-card.component";

@Component({
  selector: 'app-home',
  imports: [TopNavComponent, SongCardComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit{

songCards = [
    {
      thumbnail: "assets/song_placeholder.png",
      title: 'Test song',
      description: 'Test einer song description oder so',
    },
    {
      thumbnail: "assets/song_placeholder.png",
      title: 'Test song',
      description: 'Test einer song description oder so',
    }
  ];

  public test = "tsadfasfasdfsadf";
  public thumbnail = "assets/song_placeholder.png";

  ngOnInit(): void {
    
  }
}
