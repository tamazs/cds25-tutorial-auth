import {
  createBrowserRouter,
  createRoutesFromElements,
  Route,
} from "react-router-dom";
import Root from "./root";
import Index from "./pages";
import ErrorPage from "./pages/error";

const router = createBrowserRouter(
  createRoutesFromElements(
    <Route path="/" element={<Root />} errorElement={<ErrorPage />}>
      <Route index={true} element={<Index />} />
      {/* <Route index={true} element={<Index />} loader={indexLoader} /> */}
      {/* <Route path="/login" element={<Login />} /> */}
      {/* <Route path="/register" element={<Register />} /> */}
      {/* <Route path="/register-success" element={<RegisterSuccess />} /> */}
      {/* <Route path="/post"> */}
      {/*   <Route path=":id" element={<Post />} loader={postLoader}></Route> */}
      {/* </Route> */}
      {/* <Route path="/draft"> */}
      {/*   <Route path="" element={<DraftList />} loader={draftsLoader}></Route> */}
      {/*   <Route path="create" element={<DraftCreate />}></Route> */}
      {/*   <Route */}
      {/*     path=":id" */}
      {/*     element={<DraftUpdate />} */}
      {/*     loader={draftLoader} */}
      {/*   ></Route> */}
      {/* </Route> */}
    </Route>,
  ),
);

export default router;
