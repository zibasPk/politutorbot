import React from 'react';
import styles from "./ActiveTutorings.module.css";
import configData from "../../config/config.json";


import RefreshableComponent from '../Interfaces';
import Table from '../utils/Table';
import ActiveTutoringsModal from './ActiveTutoringsModal';
import EndedTutoringsModal from './EndedTutoringsModal';

import CircularProgress from '@mui/material/CircularProgress';
import ManualActivation from './ManualActivation';
import { makeCall } from '../../utils/MakeCall';

const ActiveHeaders = {
  TutorCode: "Cod. Matr. Tutor",
  TutorSurname: "Cognome Tutor",
  TutorName: "Nome Tutor",
  ExamCode: "Codice Esame",
  StudentCode: "Cod. Matr. Studente",
  StartDate: "Data Inizio",
}

const EndedHeaders = {
  TutorCode: "Cod. Matr. Tutor",
  TutorSurname: "Cognome Tutor",
  TutorName: "Nome Tutor",
  ExamCode: "Codice Esame",
  StudentCode: "Cod. Matr. Studente",
  StartDate: "Data Inizio",
  EndDate: "Data Fine",
  Duration: "Durata in Ore"
}



class ActiveTutorings extends RefreshableComponent
{
  constructor(props)
  {
    super(props);
    this.state = {
      ActiveTutoringsArray: undefined,
      EndedTutoringsArray: undefined
    }
  }

  async refreshData()
  {
    let status = { code: 0 };

    // fetch and format active tutorings
    let activeTutorings = await makeCall({ url: configData.botApiUrl + '/tutoring/active', method: "GET", hasAuth: true, status: status });
    
    const formatActiveTutoring = (tutoring) => {
      if (tutoring.ExamCode === null)
          tutoring.ExamCode = "OFA"
        tutoring.StartDate = new Date(tutoring.StartDate);
    }

    activeTutorings.forEach((elem) =>
    {
      formatActiveTutoring(elem);
    });
    this.setState({
      ActiveTutoringsArray: activeTutorings,
    });

    // fetch and format ended tutorings
    let endedTutorings = await makeCall({ url: configData.botApiUrl + '/tutoring/ended', method: "GET", hasAuth: true, status: status });

    endedTutorings.forEach((elem) =>
    {
      formatActiveTutoring(elem);
      elem.EndDate = new Date(elem.EndDate);
    });

    this.setState({
      EndedTutoringsArray: endedTutorings,
    });
  }

  

  render()
  {
    return (
      <>
        <div className={styles.content} >
          <h1>Attivazione Manuale</h1>
          <ManualActivation onChange={() => this.refreshData()} />
          <h1>Tutoraggi Attivi</h1>
          {this.state.ActiveTutoringsArray === undefined ? <div className='pendingDiv'><CircularProgress /></div>:
            <div className='contentWithBg'>
              <Table headers={ActiveHeaders}
                content={this.state.ActiveTutoringsArray} hasChecks={true}
                modalProps={{
                  modalContent: ActiveTutoringsModal,
                  modalTitle: "Concludi Tutoraggi selezionati",
                  onModalEvent: () => this.refreshData()
                }} />
            </div>
          }
          <h1>Tutoraggi Conclusi</h1>
          {this.state.EndedTutoringsArray === undefined ? <div className='pendingDiv'><CircularProgress /></div> :
            <div className='contentWithBg'>
              <Table headers={EndedHeaders}
                content={this.state.EndedTutoringsArray} hasChecks={true}
                modalProps={{
                  modalContent: EndedTutoringsModal,
                  modalTitle: "Esporta File CSV",
                }}
              />
            </div>}

        </div>
      </>
    );
  }
}

export default ActiveTutorings;