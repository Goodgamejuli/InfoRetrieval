import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent implements OnInit {
  
  public usernameFormControl = new FormControl(null, [Validators.required]);
  public passwordFormControl = new FormControl(null, [Validators.minLength(4)]);

  public userForm!: FormGroup;

  ngOnInit(): void {
    this.userForm = new FormGroup({
      username: this.usernameFormControl,
      password: this.passwordFormControl,
    });
  }

  submit() {
    console.log(this.userForm.value);
  }
}
