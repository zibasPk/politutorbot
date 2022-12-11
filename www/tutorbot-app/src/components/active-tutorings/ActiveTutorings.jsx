import React from 'react';
import styles from "./ActiveTutorings.module.css"
import configData from "../../config/config.json"

import RefreshableComponent from '../Interfaces';
import Table from '../utils/Table';
import ActiveTutoringsModal from './ActiveTutoringsModal';
import EndedTutoringsModal from './EndedTutoringsModal';

import CircularProgress from '@mui/material/CircularProgress';

const TutoringsArray = [
  {
    id: 1,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    start_date: new Date('2000-12-17T03:24:00'),
    end_date: new Date('1995-12-17T03:24:00'),
    duration: 14,
    selected: false
  },
  {
    id: 2,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: null,
    duration: null,
    selected: false
  },
  {
    id: 3,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: 11999,
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: new Date('1995-12-17T03:24:00'),
    duration: 36,
    selected: false
  },
  {
    id: 4,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: 11999,
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: new Date('1995-12-17T03:24:00'),
    duration: 11,
    selected: false,
  },
  {
    id: 5,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: 11999,
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: new Date('1995-12-17T03:24:00'),
    duration: 9,
    selected: false,
  },
  {
    id: 6,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: 11999,
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: null,
    duration: null,
    selected: false,
  },
  {
    id: 7,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: 11999,
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: new Date('1995-12-17T03:24:00'),
    duration: 22,
    selected: false,
  },
  {
    id: 8,
    tutorNumber: 321321,
    tutorSurname: "Brontesi",
    tutorName: "Mario Claudiolini",
    examCode: 11999,
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: null,
    duration: null,
    selected: false,
  },
  {
    id: 9,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: 11999,
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: null,
    duration: null,
    selected: false,
  },
  {
    id: 10,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: 11999,
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: null,
    duration: null,
    selected: false
  },
  {
    id: 11,
    tutorNumber: 431321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: 11999,
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: null,
    duration: null,
    selected: false,
  },
];

const ActiveHeaders = {
  TutorCode: "Cod. Matr. Tutor",
  TutorSurname: "Cognome Tutor",
  TutorName: "Nome Tutor",
  ExamCode: "Cod. Matr. Studente",
  StudentCode: "Codice Esame",
  StartDate: "Data Inizio",
}

const EndedHeaders = {
  TutorCode: "Cod. Matr. Tutor",
  TutorSurname: "Cognome Tutor",
  TutorName: "Nome Tutor",
  ExamCode: "Cod. Matr. Studente",
  StudentCode: "Codice Esame",
  StartDate: "Data Inizio",
  EndDate: "Data Fine",
  Duration: "Durata in Ore"
}

class ActiveTutorings extends RefreshableComponent {
  constructor(props) {
    super(props);
    this.state = {
      ActiveTutoringsArray: undefined,
      EndedTutoringsArray: undefined
    }
  }

  refreshData() {
    console.log("refreshing data");
    fetch(configData.botApiUrl + '/tutoring/active', {
      method: 'GET',
      headers: {
        'Authorization': 'Basic ' + btoa(configData.authCredentials),
      }
    }).then(resp => resp.json())
      .then((tutorings) => {
        tutorings.forEach((elem) => {
          if (elem.ExamCode === null)
            elem.ExamCode = "OFA"
          elem.StartDate = new Date(elem.StartDate);
        });
        this.setState({
          ActiveTutoringsArray: tutorings,
        })
      })

    fetch(configData.botApiUrl + '/tutoring/ended', {
      method: 'GET',
      headers: {
        'Authorization': 'Basic ' + btoa(configData.authCredentials),
      }
    }).then(resp => resp.json())
      .then((tutorings) => {
        tutorings.forEach((elem) => {
          if (elem.ExamCode === null)
            elem.ExamCode = "OFA"
          elem.StartDate = new Date(elem.StartDate);
          elem.EndDate = new Date(elem.EndDate);
        });

        this.setState({
          EndedTutoringsArray: tutorings,
        })
      })
  }

  render() {
    return (
      <>
        <div className={styles.content} >
          <h1>Tutoraggi Attivi</h1>
          {this.state.ActiveTutoringsArray === undefined ? <CircularProgress /> :
            <>
              <Table headers={ActiveHeaders}
                content={this.state.ActiveTutoringsArray} hasChecks={true}
                modalProps={{
                  modalContent: ActiveTutoringsModal,
                  modalTitle: "Concludi Tutoraggi selezionati",
                  onModalEvent: () => this.refreshData()
                }} />
            </>
          }
          <h1>Tutoraggi Conclusi</h1>
          {this.state.EndedTutoringsArray === undefined ? <CircularProgress /> :
            <>
              <Table headers={EndedHeaders}
                content={this.state.EndedTutoringsArray} hasChecks={true}
                modalProps={{
                  modalContent: EndedTutoringsModal,
                  modalTitle: "Esporta File CSV"
                }}
              />
            </>}
        </div>
      </>
    );
  }
}

export default ActiveTutorings;