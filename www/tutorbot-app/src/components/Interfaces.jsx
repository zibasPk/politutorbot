import React, { Component } from 'react';



class RefreshableComponent extends Component {
  constructor(props) {
    super(props);
  }
  
  componentDidMount() {
    this.refreshData();
  }

  refreshData() {
    throw new Error("Method 'refreshData()' must be implemented.");
  }
}
 
export default RefreshableComponent;