import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-top-nav',
  imports: [],
  templateUrl: './top-nav.component.html',
  styleUrl: './top-nav.component.css'
})
export class TopNavComponent implements OnInit{


  constructor(private router: Router) {}
  
  ngOnInit(): void {}

  onNavigateToLogin() {
    console.log("Test");
    this.router.navigate(['/', 'login'])
  }
}
