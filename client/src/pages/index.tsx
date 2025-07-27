import { BlogClient, type Post } from "../models/generated-client";
import WelcomeHero from "../components/base/welcome-hero";
import { useLoaderData } from "react-router-dom";
import PostPreview from "../components/post/post-preview";

export async function indexLoader() {
  return await new BlogClient().list(0);
}

export default function Index() {
  const posts = useLoaderData() as Post[];

  return (
    <>
      <WelcomeHero />
      <div className="divider"></div>
      {posts ? (
        posts.map((post) => <PostPreview key={post.id} post={post} />)
      ) : (
        <p>No posts found</p>
      )}
    </>
  );
}
