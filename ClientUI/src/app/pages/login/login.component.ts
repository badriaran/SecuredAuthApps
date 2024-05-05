import { ParseSourceFile } from '@angular/compiler';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatIcon } from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input'
import { RouterLink } from '@angular/router';
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [MatInputModule, MatIcon, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent implements OnInit{
hide=true;
form!:FormGroup;
fb=inject(FormBuilder);

login(){

}
ngOnInit(): void {
  //Called after the constructor, initializing input properties, and the first call to ngOnChanges.
  //Add 'implements OnInit' to the class.
  this.form=this.fb.group({
    email:['',[Validators.required, Validators.email]],
    password:['',Validators.required]
  })
};

}
