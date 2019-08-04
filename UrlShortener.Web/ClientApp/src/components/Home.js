import React from 'react';
import { connect } from 'react-redux';

const Home = props => (
  <div>
    <h1>Smaller Url</h1>
        <p>Welcome to smaller url, a sample url shortening service.</p>

        <p>The swagger endpoint for the api can be found: <a href='/swagger/index.html'>here</a></p>
  </div>
);

export default connect()(Home);
