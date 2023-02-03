import React from 'react';
import styles from './EnabledStudents.module.css';
import configData from "../../config/config.json"
import validationConfig from "../../config/validation-config.json"

import Papa from "papaparse";

import Form from 'react-bootstrap/Form';
import RefreshableComponent from '../Interfaces';
import { CircularProgress } from '@mui/material';
import FileUploadIcon from '@mui/icons-material/FileUpload';
import { makeCall } from '../../MakeCall';
import UploadForm from '../utils/UploadForm';

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

  async refreshData()
  {
    let status = { code: 0 };
    let result = await makeCall({ url: configData.botApiUrl + "/students", method: "GET", hasAuth: true, status: status });
    if (status.code !== 200)
    {
      return;
    }

    this.setState({
      EnabledStudents: result
    });
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

  async enabledStudent()
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

    let status = { code: 0 };
    let result = await makeCall({ url: configData.botApiUrl + "/students/enable/" + this.state.StudentToEnable, method: "POST", hasAuth: true, status: status });

    if (status.code !== 200)
    {
      this.setState({
        AlertText: result,
      })
      return;
    }

    this.refreshData();
    this.setState({
      AlertText: ""
    });
  }

  async disableStudent()
  {
    if (this.state.StudentToDisable == null || !this.state.StudentToDisable.toString().match(validationConfig.studentCodeRegex))
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

    let status = { code: 0 };
    let result = await makeCall({ url: configData.botApiUrl + "/students/disable/" + this.state.StudentToDisable, method: "POST", hasAuth: true, status: status });

    if (status.code !== 200)
    {
      this.setState({
        AlertText: result,
      })
      return;
    }

    this.refreshData();
    // hide the alert on success
    this.setState({
      AlertText: ""
    });
  }

  parseStudentsFile(file, alertSetter, sendFile)
  {
    // If user clicks the parse button without
    // a file we show a error
    if (!file)
    {
      alertSetter("Nessun file selezionato");
      return;
    };

    // Initialize a reader which allows user
    // to read any file or blob.
    const reader = new FileReader();


    // Event listener on reader when the file
    // loads, we parse it and send the data.
    reader.onload = async ({ target }) =>
    {
      let alertMsg = null;

      const csv = Papa.parse(target.result, { header: false, skipEmptyLines: true });
      const parsedData = csv?.data;
      const formattedData = parsedData.map((line) => line[0]);

      formattedData.forEach(element =>
      {
        alertMsg = this.validateStudent(element);
        if (alertMsg != null)
        {
          alertSetter("Errore nei dati inseriti: " + alertMsg);
          return;
        }
      });

      if (alertMsg == null)
      {
        sendFile(formattedData);
        return;
      }

    };
    reader.readAsText(file);
  }

  validateStudent(student)
  {
    if (!student.toString().match(validationConfig.studentCodeRegex))
    {
      return "Codice matricola " + student + " non valido";
    }
    return null;
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
              <h1>Abilita Studenti</h1>
              <Form.Group controlId="formTextEnable" className="mb-3">
                <Form.Label>Abilita Studente</Form.Label>
                <div className={styles.inputDiv}>
                  <Form.Control type="text" placeholder="Matr. Studente"
                    onChange={(e) => this.changeStudentToEnable(parseInt(e.target.value))}
                  />
                  <FileUploadIcon className={styles.actionBox} onClick={() => this.enabledStudent()} />
                </div>
              </Form.Group>
              <UploadForm
                formText="Carica File CSV con le matricole da abilitare"
                infoContent={
                  <>
                    <div>Caricare un file CSV contente un elenco <strong>in colonna</strong> di codici matricola da abilitare.</div>
                  </>}
                uploadEndPoint="/students/enable"
                parseData={(file, alertSetter, sendFile) => this.parseStudentsFile(file, alertSetter, sendFile)}
                callBack={() => this.refreshData()}
              />
            </div>
            <div className={styles.removeFunctions}>
              <h1>Rimuovi Studenti</h1>
              <Form.Group controlId="formTextRemove" className="mb-3">
                <Form.Label>Rimuovi Studente</Form.Label>
                <div className={styles.inputDiv}>
                  <Form.Control type="text" placeholder="Matr. Studente"
                    onChange={(e) => this.changeStudentToDisable(parseInt(e.target.value))}
                  />
                  <FileUploadIcon className={styles.actionBox} onClick={() => this.disableStudent()} />
                </div>
              </Form.Group>
              <UploadForm
                formText="Carica File CSV con le matricole da rimuovere"
                infoContent={
                  <>
                    <div>Caricare un file CSV contente un elenco <strong>in colonna</strong> di codici matricola da rimuovere.</div>
                  </>}
                uploadEndPoint="/students/disable"
                parseData={(file, alertSetter, sendFile) => this.parseStudentsFile(file, alertSetter, sendFile)}
                callBack={() => this.refreshData()}
              />
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

