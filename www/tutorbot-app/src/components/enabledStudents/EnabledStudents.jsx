import React from 'react';
import styles from './EnabledStudents.module.css';
import configData from "../../config/config.json"

import Form from 'react-bootstrap/Form';
import InfoIcon from '../utils/InfoIcon';
import RefreshableComponent from '../Interfaces';
import { CircularProgress } from '@mui/material';
import CheckIcon from '@mui/icons-material/Check';

export default class EnabledStudents extends RefreshableComponent
{
  constructor(props)
  {
    super(props);
    this.state = {
      EnabledStudents: undefined,
      StudentToEnable: null,
      StudentToDisable: null,
      AlertText: ""
    };
  }


  refreshData()
  {
    fetch(configData.botApiUrl + '/students', {
      method: 'GET',
      headers: {
        'Authorization': 'Basic ' + btoa(configData.authCredentials),
      }
    }).then(resp => resp.json())
      .then((students) =>
      {
        this.setState({
          EnabledStudents: students,
        })
      })
  }

  changeStudentToEnable(value)
  {
    if (isNaN(value))
    {
      this.setState({
        StudentToEnable: null,
      })
      return;
    }

    this.setState({
      StudentToEnable: value,
      AlertText: ""
    })

  }

  changeStudentToDisable(value)
  {
    if (isNaN(value))
    {
      this.setState({
        StudentToDisable: null,
        AlertText: ""
      })
      return;
    }

    this.setState({
      StudentToDisable: value,
    })

  }

  enabledStudent()
  {
    if (this.state.StudentToEnable == null || !this.state.StudentToEnable.toString().match(/^[1-9][0-9]{5}$/))
    {
      this.setState({
        AlertText: "Inserire un codice matricola valido",
      })
      return;
    }

    if(this.state.EnabledStudents.includes(this.state.StudentToEnable)) {
      this.setState({
        AlertText: "Il codice matricola inserito è già abilitato",
      })
      return;
    }

    fetch(configData.botApiUrl + '/students/enable/' + this.state.StudentToEnable, {
      method: 'POST',
      headers: {
        'Authorization': 'Basic ' + btoa(configData.authCredentials),
      }
    }).then(resp =>
    {
      if (!resp.ok)
        return resp.text();
      this.refreshData();
    })
      .then((text) =>
      {
        if (text !== undefined)
        {
          this.setState({
            AlertText: text,
          })
          return;
        }
        // Hide alert after a positive response
        this.setState({
          AlertText: ""
        })
      })
  }

  disableStudent()
  {
    if (this.state.StudentToDisable == null || !this.state.StudentToDisable.toString().match(/^[1-9][0-9]{5}$/))
    {
      this.setState({
        AlertText: "Inserire un codice matricola valido",
      })
      return;
    }

    if(!this.state.EnabledStudents.includes(this.state.StudentToDisable)) {
      this.setState({
        AlertText: "Il codice matricola inserito è già non abilitato.",
      })
      return;
    }

    fetch(configData.botApiUrl + '/students/disable/' + this.state.StudentToDisable, {
      method: 'POST',
      headers: {
        'Authorization': 'Basic ' + btoa(configData.authCredentials),
      }
    }).then(resp =>
    {
      if (!resp.ok)
        return resp.text();
      this.refreshData();
    })
      .then((text) =>
      {
        if (text !== undefined)
        {
          this.setState({
            AlertText: text,
          })
          return;
        }
        // Hide alert after a positive response
        this.setState({
          AlertText: ""
        })
      })
  }

  render()
  {
    return (
      <>
        <div className={styles.content}>
          {this.state.AlertText !== "" ?
            <div className={styles.alertText}>{this.state.AlertText}</div>
            : <></>
          }
          <div className={styles.functionsHeader}>
            <div className={styles.addFunctions}>
              <h1>Abilita studenti</h1>
              <Form.Group controlId="formTextEnable" className="mb-3">
                <Form.Label>Abilita Studente</Form.Label>
                <div className={styles.inputDiv}>
                  <Form.Control type="text" placeholder="Matr. Studente"
                    onChange={(e) => this.changeStudentToEnable(parseInt(e.target.value))}
                  />
                  <CheckIcon className={styles.actionBox} onClick={() => this.enabledStudent()} />
                </div>

              </Form.Group>
              <Form.Group controlId="formFileEnable" className="mb-3">
                <Form.Label>Carica File CSV</Form.Label>
                <InfoIcon text="Caricare un file CVS contente un elenco di codici matricola da abilitare." />
                <Form.Control type="file" />
              </Form.Group>
            </div>
            <div className={styles.removeFunctions}>
              <h1>Rimuovi studenti</h1>
              <Form.Group controlId="formTextRemove" className="mb-3">
                <Form.Label>Rimuovi Studente</Form.Label>
                <div className={styles.inputDiv}>
                  <Form.Control type="text" placeholder="Matr. Studente"
                    onChange={(e) => this.changeStudentToDisable(parseInt(e.target.value))}
                  />
                  <CheckIcon className={styles.actionBox} onClick={() => this.disableStudent()}/>
                </div>
              </Form.Group>
              <Form.Group controlId="formFileRemove" className="mb-3">
                <Form.Label>Carica File CSV</Form.Label>
                <InfoIcon text="Caricare un file CVS contente un elenco di codici matricola da rimuovere." />
                <Form.Control type="file" />
              </Form.Group>
            </div>
          </div>
          {this.state.EnabledStudents === undefined ? <CircularProgress /> :
            <StudentList studentArray={this.state.EnabledStudents} />}
        </div>
      </>
    );
  }
}

class StudentList extends React.Component
{
  constructor(props)
  {
    super(props);
    this.state = {
      Students: props.studentArray,
      FilteredStudents: props.studentArray,
      ActiveFilter: ""
    };
  }

  filterList(event)
  {
    const tempList = this.state.Students.filter(
      (res) => res.toString().includes(event.target.value)
    );

    this.setState({
      FilteredStudents: tempList,
      ActiveFilter: event.target.value
    })
  }

  static getDerivedStateFromProps(props, state) {
    console.log("hola");
    if (props.studentArray !== state.Students) {
      //Change in props
      const tempList = props.studentArray.filter(
        (student) => student.toString().includes(state.ActiveFilter)
      );

      return {
        Students: props.studentArray,
        FilteredStudents: tempList
      };
    }
    return null; // No change to state
  }

  render()
  {
    return (
      <div>
        <h2>Studenti Abilitati</h2>
        <Form.Group controlId="formTextSearch" className="mb-3">
          <Form.Label>Cerca Studente</Form.Label>
          <Form.Control type="text" placeholder="Matr. Studente" onChange={(e) => this.filterList(e)} />
        </Form.Group>
        <div className={styles.resultAlert}>{this.state.FilteredStudents.length} risultati</div>
        <div className={styles.listContainer}>
          <ul className={styles.studentList}>
            {this.state.FilteredStudents.map((s) => (<li key={s}>{s}</li>))}
          </ul>
        </div>
      </div>
    )
  }
}

