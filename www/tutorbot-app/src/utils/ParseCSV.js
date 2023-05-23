import Papa from "papaparse";

function parseCSV(file, alertSetter, options = { header: true , skipEmptyLines: true})
  {
    // If user clicks the parse button without
    // a file we show a error
    if (!file)
    {
      alertSetter("Nessun file selezionato");
      return;
    };


    Papa.parse(file, options);
  }