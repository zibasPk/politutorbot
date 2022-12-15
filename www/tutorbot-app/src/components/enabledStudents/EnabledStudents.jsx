import React from 'react';
import styles from './EnabledStudents.module.css';
import configData from "../../config/config.json"

import Papa from "papaparse";

import Form from 'react-bootstrap/Form';
import InfoIcon from '../utils/InfoIcon';
import RefreshableComponent from '../Interfaces';
import { CircularProgress } from '@mui/material';
import CheckIcon from '@mui/icons-material/Check';
import FileUploadIcon from '@mui/icons-material/FileUpload';

export const allowedExtensions = ["csv", "vnd.ms-excel"];

export default class EnabledStudents extends RefreshableComponent
{
  constructor(props)
  {
    super(props);
    this.state = {
      EnabledStudents: undefined,
      StudentToEnable: null,
      StudentToDisable: null,
      StudentsToEnableFile: null,
      StudentsToDisableFile: null,
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

  handleEnabledFileChange(e)
  {
    if (e.target.files.length)
    {
      const inputFile = e.target.files[0]

      // Check the file extensions, if it not
      // included in the allowed extensions
      // we show the error
      const fileExtension = inputFile?.type.split("/")[1];
      if (!allowedExtensions.includes(fileExtension))
      {
        this.setState(
          {
            StudentsToEnableFile: null,
            AlertText: "File inserito non del formato .csv"
          }
        )
        return;
      }

      this.setState(
        {
          StudentsToEnableFile: inputFile,
          AlertText: ""
        }
      )
    }
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

  handleToDisableFileChange(e)
  {
    if (e.target.files.length)
    {
      const inputFile = e.target.files[0]

      // Check the file extensions, if it not
      // included in the allowed extensions
      // we show the error
      const fileExtension = inputFile?.type.split("/")[1];
      if (!allowedExtensions.includes(fileExtension))
      {
        this.setState(
          {
            StudentsToDisableFile: null,
            AlertText: "File inserito non del formato .csv"
          }
        )
        return;
      }

      this.setState(
        {
          StudentsToDisableFile: inputFile,
          AlertText: ""
        }
      )
    }
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

    if (this.state.EnabledStudents.includes(this.state.StudentToEnable))
    {
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

    if (!this.state.EnabledStudents.includes(this.state.StudentToDisable))
    {
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

  sendStudents(students, action) 
  {
    // If user clicks the parse button without
    // a file we show a error
    if (!students)
    {
      this.setState({
        AlertText: "Inserire un file valido."
      })
    };

    // Initialize a reader which allows user
    // to read any file or blob.
    const reader = new FileReader();

    // Event listener on reader when the file
    // loads, we parse it and send the data.
    reader.onload = async ({ target }) =>
    {
      const csv = Papa.parse(target.result, { header: false, skipEmptyLines: true });
      const parsedData = csv?.data;
      const formattedData = parsedData.map((line) => line[0]);

      fetch(configData.botApiUrl + '/students/' + action, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Basic ' + btoa(configData.authCredentials),
        },
        body: JSON.stringify(formattedData)
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
    };
    reader.readAsText(students);
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
                  <FileUploadIcon className={styles.actionBox} onClick={() => this.enabledStudent()} />
                </div>
              </Form.Group>
              <Form.Group controlId="formFileEnable" className="mb-3">
                <Form.Label>Carica File CSV</Form.Label>
                <InfoIcon text="Caricare un file CVS contente un elenco (**in colonna**) di codici matricola da abilitare." />
                <div className={styles.inputDiv}>
                  <Form.Control type="file" onChange={(e) => this.handleEnabledFileChange(e)} />
                  <FileUploadIcon className={styles.actionBox}
                    onClick={() => this.sendStudents(this.state.StudentsToEnableFile, "enable")} />
                </div>
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
                  <FileUploadIcon className={styles.actionBox} onClick={() => this.disableStudent()} />
                </div>
              </Form.Group>
              <Form.Group controlId="formFileRemove" className="mb-3">
                <Form.Label>Carica File CSV</Form.Label>
                <InfoIcon text="Caricare un file CVS contente un elenco (**in colonna**) di codici matricola da rimuovere." />
                <div className={styles.inputDiv}>
                  <Form.Control type="file" onChange={(e) => this.handleToDisableFileChange(e)}/>
                  <FileUploadIcon className={styles.actionBox}
                    onClick={() => this.sendStudents(this.state.StudentsToDisableFile, "disable")} />
                </div>
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

  static getDerivedStateFromProps(props, state)
  {
    if (props.studentArray !== state.Students)
    {
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

