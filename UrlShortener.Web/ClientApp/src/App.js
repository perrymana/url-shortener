import React from 'react';
import { Route } from 'react-router';
import Layout from './components/Layout';
import UrlShortener from './components/UrlShortener';
import ApiDetails from './components/ApiDetails';

export default () => (
  <Layout>
        <Route exact path='/' component={UrlShortener} />
        <Route exact path='/apidetails' component={ApiDetails} />
  </Layout>
);
